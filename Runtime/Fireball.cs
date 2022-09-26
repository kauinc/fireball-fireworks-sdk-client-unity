using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fireball.Game.Client.Models;
using Fireball.Game.Client.Modules;
using Fireball.Game.Client.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Fireball.Game.Client
{
    public class Fireball : MonoBehaviour, IFireball
    {
        public static Fireball Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new GameObject("Fireball").AddComponent<Fireball>();
                            DontDestroyOnLoad(_instance.gameObject);
                        }
                    }
                }

                return _instance;
            }
        }

        public FireballSession CurrentSession => _currentSession;
        public string LastActionID => _lastActionID;

        public bool IsInit => _currentSession != null
                              && !string.IsNullOrEmpty(_currentSession.ConnectionId)
                              && _messenger.IsInit;

        public bool IsAuth => _currentSession != null
                              && !string.IsNullOrEmpty(_currentSession.GameSession)
                              && !string.IsNullOrEmpty(_currentSession.PlayerId);

        public bool IsDemo => _currentSession.GameSession.Equals(FireballConfig.DEMO_SESSION)
                              || _currentSession.GameMode == GameMode.fun.ToString();

        public Action<JackpotUpdateMessage> OnJackpotUpdate { get; set; }

        private static readonly object _syncRoot = new object();
        private static Fireball _instance;

        private ThreadDispatcher _dispatcher;
        private FireballSession _currentSession;
        private string _customRouterUrl;
        private string _lastActionID;
        private IMessenger _messenger;
        private IFireballLogger _logger;
        private INetworkChecker _networkChecker;

        private readonly Dictionary<string, string> _pendingRequests = new Dictionary<string, string>();
        private readonly Dictionary<string, JToken> _pendingResponses = new Dictionary<string, JToken>();

        private Action<FireballSession> _onInitSuccess = null;
        private Action<string> _onInitError = null;

        private string URLRouter => !string.IsNullOrEmpty(_customRouterUrl) ? _customRouterUrl : FireballConfig.URL_ROUTER_DEFAULT;

        public void Awake()
        {
            if (_instance == null) _instance = this;
        }

        public void Init(Action<FireballSession> onSuccess = null, Action<string> onError = null)
        {
            Initialize(URLData.ParseSessionFromURL(), onSuccess, onError);
        }

        public void Init(string customUrl, Action<FireballSession> onSuccess = null, Action<string> onError = null)
        {
            Initialize(URLData.ParseSessionFromURL(customUrl), onSuccess, onError);
        }

        public void Init(FireballSettings playerData, Action<FireballSession> onSuccess = null, Action<string> onError = null)
        {
            Initialize(playerData.GetSession(), onSuccess, onError);
        }

        private void Initialize(FireballSession customSession, Action<FireballSession> onSuccess = null, Action<string> onError = null, MessengerType messengerType = MessengerType.BestHTTPv2)
        {
            _logger = new FireballLogger();
            _networkChecker = new NetworkChecker(this, 2.0f);
            _dispatcher = new ThreadDispatcher(this);

            _onInitSuccess = onSuccess;
            _onInitError = onError;

            _logger.Log("Init...");
            _currentSession = customSession;
            _currentSession.ConnectionToken = FireballTools.GenerateConnectionToken();

            _customRouterUrl = _currentSession.Router;

            // Websocket module init
            if (messengerType == MessengerType.SignalR)
            {
                _messenger = new SignalRMessenger(_currentSession, this);
            }
            else if(messengerType == MessengerType.BestHTTPv2)
            {
                _messenger = new BestHTTPMessenger(this);
            }
            else
            {
                _messenger = new WebSocketMessenger(_currentSession);
            }

            _messenger.OnMessageReceived += OnMessageReceived;
            _messenger.OnConnectionChange += OnConnectionChange;

            // Send ping to warm up Fireball system
            // SendPing();

            // Start check network connection
            _networkChecker.StartNetworkCheck();
            _networkChecker.OnNetworkConnectionChanged += OnInternetConnection;

            // Connect to App Messages WebSocket server
            _messenger.Connect(_currentSession.WsServer, _currentSession.ConnectionToken,
                (connectionId) =>
                {
                    _logger.Info("OnInit: Success!");
                    _currentSession.ConnectionId = connectionId;
                    _onInitSuccess?.Invoke(_currentSession);
                    _onInitSuccess = null;
                },
                (error) =>
                {
                    _logger.Error($"OnInit: Error! {error}");
                    _onInitError?.Invoke(error);
                    _onInitError = null;
                });
        }

        public void Authorize(AuthRequest authRequest, Action<AuthResponse> onSuccess = null, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1) =>
            Authorize<AuthRequest, AuthResponse>(authRequest, onSuccess, onError);

        public void Authorize<TRequest, TResponse>(TRequest authRequest, Action<TResponse> onSuccess = null, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1) where TRequest : AuthRequest where TResponse : AuthResponse
        {
            SendRequest<TRequest, TResponse>(authRequest,
                response =>
                {
                    _currentSession.GameSession = response.GameSession;
                    _currentSession.PlayerId = response.PlayerId;
                    _currentSession.OperatorPlayerId = response.OperatorPlayerId;
                    _currentSession.OperatorPlayerSession = response.OperatorPlayerSession;
                    onSuccess?.Invoke(response);
                },
                onError, timeout, attempts);
        }

        public void DemoAuthorize(string currency = FireballConfig.DEFAULT_CURRENCY, long balance = FireballConfig.DEMO_BALANCE, Action<AuthResponse> onSuccess = null, Action<ErrorResponse> onError = null)
        {
            try
            {
                _currentSession = new FireballSession();
                _currentSession.GameMode = GameMode.fun.ToString();
                _currentSession.GameSession = FireballConfig.DEMO_SESSION;
                _currentSession.PlayerId = FireballConfig.DEMO_USER_ID;

                var response = new AuthResponse();
                response.Balance = balance;
                response.Currency = currency;

                onSuccess?.Invoke(response);
            }
            catch(Exception e)
            {
                onError?.Invoke(ErrorResponse.CustomError(null, e.Message, 0));
            }
        }

        public void SendPing() =>
            SendPOST(URLRouter, new PingRequest(_currentSession));

        private void OnInternetConnection(bool connected)
        {
            if (!IsInit && !connected)
            {
                return;
            }

            //_fireballLogger.Log($"On Connection change: connected = {connected}");

            if (_messenger is { IsClosed: true })
            {
                _messenger.Reconnect();
            }
        }

        private void OnDestroy()
        {
            if (_networkChecker != null)
            {
                _networkChecker.StopNetworkCheck();
                _networkChecker.OnNetworkConnectionChanged -= OnInternetConnection;
            }

            if (_messenger != null)
            {
                _messenger.Disconnect();
            }
        }

        public void SendGET(string url, Dictionary<string, string> data = null, Action<string> onSuccess = null, Action<string> onError = null) =>
            StartCoroutine(SendGETCoroutine(url, data, onSuccess, onError));

        private IEnumerator SendGETCoroutine(string url, Dictionary<string, string> data, Action<string> onSuccess, Action<string> onError)
        {
            long responceCode = 0;
            string responceText = string.Empty;
            url = FireballTools.FormatUrlAndParams(url, data);

            _logger.Info($"Sending GET Request to URL = {url}");
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();
            using (UnityWebRequest client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null))
            {
                client.SetRequestHeader("Content-Type", "application/json");
                yield return client.SendWebRequest();

                if (client.result == UnityWebRequest.Result.ConnectionError ||
                    client.result == UnityWebRequest.Result.ProtocolError)
                {
                    responceText = client.error;
                    responceCode = client.responseCode;
                    _logger.Error($"GET Request Error: {responceText} ({responceCode})");
                    onError?.Invoke(responceText);
                }
                else
                {
                    responceText = client.downloadHandler?.text;
                    responceCode = client.responseCode;
                    _logger.Info($"GET Response: {responceText} ({responceCode})");
                    onSuccess?.Invoke(responceText);
                }
            }
        }

        public void SendPOST(string url, BaseMessage request, Action<string> onSuccess = null, Action<string> onError = null) =>
            StartCoroutine(SendPOSTCoroutine(url, request, onSuccess, onError));

        private IEnumerator SendPOSTCoroutine(string url, BaseMessage request, Action<string> onSuccess, Action<string> onError)
        {
            long responceCode = 0;
            string responceText = string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(request.ToJson());

            _logger.Info($"Message - {request.Name} - Sending... (ActionId: {request.ActionId})" +
                $"\nMesaage: {request.ToJson()}");

            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.ToJson()));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler))
            {
                client.SetRequestHeader("Content-Type", "application/json");
                client.SetRequestHeader("Accept", "application/json");
                client.uploadHandler.contentType = "application/json";

                yield return client.SendWebRequest();

                if (client.result == UnityWebRequest.Result.ConnectionError ||
                    client.result == UnityWebRequest.Result.ProtocolError)
                {
                    responceText = client.error;
                    responceCode = client.responseCode;
                    _logger.Error($"Message - {request.Name} - Error: {responceText} (ActionId: {request.ActionId})");
                    onError?.Invoke(responceText);
                }
                else
                {
                    responceText = client.downloadHandler.text;
                    responceCode = client.responseCode;
                    _logger.Info($"Message - {request.Name} - Sent (ActionId: {request.ActionId})");
                    onSuccess?.Invoke(responceText);
                }
            }
        }

        public void SendRequest<TRequest, TResponse>(TRequest request, Action<TResponse> onSuccess, Action<ErrorResponse> onError = null, float timeout = FireballConfig.DEFAULT_TIMEOUT, int attempts = 1)
            where TRequest : BaseRequest
            where TResponse : BaseResponse =>
            StartCoroutine(SendRequestCoroutine(request, onSuccess, onError, timeout, attempts));

        private IEnumerator SendRequestCoroutine<TRequest, TResponse>(TRequest request, Action<TResponse> onSuccess, Action<ErrorResponse> onError, float timeout, int attemptsCount)
            where TRequest : BaseRequest
            where TResponse : BaseResponse
        {
            if (_messenger == null || !_messenger.IsConnected)
            {
                _logger.Error("Can't send request! Fail to connect to server");
                onError?.Invoke(ErrorResponse.CustomError(request.ActionId, ErrorResponse.NO_CONNECTION_REASON));
                yield break;
            }

            //CheckAndClearPendingResponses();
            _lastActionID = request.ActionId;

            float timePassed = 0;
            int attemptsLeft = attemptsCount - 1;
            _pendingRequests.Add(request.ActionId, request.Name);

            if (!IsPendingResponse(request.ActionId))
            {
                SendPOST(URLRouter, request, null,
                    errorReason =>
                    {
                        var error = ErrorResponse.CustomError(request.ActionId, errorReason);
                        AddPendingResponse(request.ActionId, JToken.FromObject(error));
                    });

                while (!IsPendingResponse(request.ActionId))
                {
                    if (timeout > 0 && timePassed >= timeout)
                    {
                        if (attemptsLeft > 0)
                        {
                            attemptsLeft--;
                            timePassed = 0;
                            _logger.Warning($"Timeout! Try next attempt... ({attemptsCount - attemptsLeft})");
                            SendPOST(URLRouter, request);
                        }
                        else
                        {
                            _logger.Error($"Timeout for message {request.Name} {request.ActionId}! " +
                                          $"Time passed: {timePassed} sec, Attempts = {attemptsCount - attemptsLeft}");
                            var timeoutError = ErrorResponse.TimeoutResponse(request.ActionId, timeout);
                            AddPendingResponse(request.ActionId, JToken.FromObject(timeoutError));
                        }
                    }
                    else
                    {
                        yield return null;
                        timePassed += Time.deltaTime;
                    }
                }
            }
            else
            {
                _logger.Log($" Found Pending Response!");
            }

            var responseObject = GetPendingResponse(request.ActionId);
            try
            {
                string errorReason = responseObject[nameof(ErrorResponse.Reason)]?.ToString();
                if (string.IsNullOrEmpty(errorReason))
                {
                    var response = responseObject.ToObject<TResponse>();
                    _logger.Info($"Message - {response.Name} - Received (ActionId: {response.ActionId})" +
                        $"\nMessage: {response.ToJson()}" +
                        $"\nTime passed: {timePassed:F1} sec, Attempts: {attemptsCount}");

                    onSuccess?.Invoke(response);
                }
                else
                {
                    var error = responseObject.ToObject<ErrorResponse>();
                    _logger.Error($"Message - {error.Name} - Error (ActionId: {error.ActionId})" +
                        $"\nError: {error.ToJson()}" +
                        $"\nTime passed: {timePassed:F1} sec, Attempts: {attemptsCount}");

                    onError?.Invoke(error);
                }
            }
            catch(Exception e)
            {
                _logger.Error(e.Message);
                onError?.Invoke(ErrorResponse.CustomError(request.ActionId, e.Message));
            }
            

            if (_pendingRequests.ContainsKey(request.ActionId))
            {
                _pendingRequests.Remove(request.ActionId);
            }

            if (_pendingResponses.ContainsKey(request.ActionId))
            {
                _pendingResponses.Remove(request.ActionId);
            }

            CheckAndClearPendingResponses();
        }

        private bool IsPendingResponse(string actionID) =>
            _pendingResponses.ContainsKey(actionID) ||
            string.Join(",", _pendingResponses.Keys).Contains(actionID);

        private JToken GetPendingResponse(string actionID)
        {
            if (_pendingResponses.ContainsKey(actionID))
            {
                return _pendingResponses[actionID];
            }

            foreach (string id in _pendingResponses.Keys)
            {
                if (id.Contains(actionID) || actionID.Contains(id))
                {
                    return _pendingResponses[id];
                }
            }

            return null;
        }

        private void AddPendingResponse(string actionID, JToken response)
        {
            //_fireballLogger.Log($"Set Pending response actionID = {actionID}");
            if (_pendingResponses.ContainsKey(actionID))
            {
                _pendingResponses[actionID] = response;
            }
            else
            {
                _pendingResponses.Add(actionID, response);
            }
        }

        private void CheckAndClearPendingResponses()
        {
            List<string> responses = new List<string>();

            foreach (string actionID in _pendingResponses.Keys)
            {
                if (!_pendingRequests.ContainsKey(actionID))
                {
                    responses.Add(actionID);
                }
            }

            if (responses.Count > 0)
            {
                _logger.Log($"Clearing Pending response = {string.Join(",", responses)}");
                foreach (string actionID in responses)
                {
                    _pendingResponses.Remove(actionID);
                }
            }
        }

        private void OnMessageReceived(string json)
        {
            //_fireballLogger.Log($"On Message Received: {json}");
            try
            {
                var data = JObject.Parse(json);
                if (data == null)
                {
                    _logger.Error("On Message error: can't parse json!");
                    return;
                }

                var actionId = data[nameof(ResponseMessageWrapper<BaseResponse>.ActionId)]?.ToString();
                var messageObject = data[nameof(ResponseMessageWrapper<BaseResponse>.Message)];
                var name = messageObject?[nameof(JackpotUpdateMessage.name)]?.ToString();

                if (name == JackpotUpdateMessage.MESSAGE_NAME)
                {
                    var jackpotMessage = messageObject.ToObject<JackpotUpdateMessage>();
                    _logger.Info($"On Jackpot Message Received: {jackpotMessage?.ToJson()}");
                    _dispatcher.InvokeInMainThread(() =>
                    {
                        OnJackpotUpdate?.Invoke(jackpotMessage);
                    });
                }
                else
                {
                    if (string.IsNullOrEmpty(actionId))
                    {
                        _logger.Error("On Message error: actionID == null!");
                        AddPendingResponse(_lastActionID, messageObject);
                    }
                    else
                    {
                        AddPendingResponse(actionId, messageObject);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.Error($"On Message error: can't parse json! Exception: {e.Message}");
            }
        }

        private void OnConnectionChange(bool isConnected, string connectionId)
        {
            if (isConnected)
            {
                _currentSession.ConnectionId = connectionId;

                _onInitSuccess?.Invoke(_currentSession);
                _onInitSuccess = null;
            }
        }

        public void GetTransactionsList(Action<TransactionsList> onSuccess, Action<string> onError = null, int startIndex = 0, bool includeGameStates = true)
        {
            if (!IsAuth)
            {
                _logger.Error($"Transactions: Error - Fireball is not authorized!");
                onError?.Invoke("Fireball is not authorized!");
                return;
            }

            var url = $"{FireballConfig.URL_REPLAY_TRANSACTION}/{CurrentSession.ConnectionId}/{includeGameStates}/{startIndex}";
            SendGET(url, null,
                (json) =>
                {
                    try
                    {
                        _logger.Log($"On Transactions: success! {json}");
                        var result = JsonConvert.DeserializeObject<TransactionsList>(json, new JsonSerializerSettings()
                        {
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        });

                        onSuccess?.Invoke(result);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"On Transactions: Error - can't parse json! Exception: {e.Message}");
                        onError?.Invoke(e.Message);
                    }
                },
                onError);
        }

        public void InvokeInMainThread(Action action, float delay = 0)
        {
            _dispatcher.InvokeInMainThread(() =>
            {
                StartCoroutine(InvokeCoroutine(action, delay));
            });
        }

        private IEnumerator InvokeCoroutine(Action action, float delay)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            action?.Invoke();
        }
    }
}
