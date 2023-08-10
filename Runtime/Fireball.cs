using System;
using System.Collections;
using System.Collections.Generic;
using Fireball.Game.Client.Models;
using Fireball.Game.Client.Modules;
using Fireball.Game.Client.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

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
                            _instance = new GameObject(nameof(Fireball)).AddComponent<Fireball>();
                            DontDestroyOnLoad(_instance.gameObject);
                        }
                    }
                }

                return _instance;
            }
        }
        public FireballMultiplayer Multiplayer
        {
            get
            {
                if (_multiplayer == null)
                {
                    _multiplayer = new FireballMultiplayer(this);
                }
                return _multiplayer;
            }
        }
        public Translation Translation
        {
            get
            {
                if (_translation == null)
                {
                    _translation = new Translation(Communicator, _logger);
                }
                return _translation;
            }
        }
        public Communicator Communicator
        {
            get
            {
                if (_communicator == null)
                {
                    _communicator = new Communicator(this, _logger);
                }
                return _communicator;
            }
        }
        public FireballGCI GameClientInterface
        {
            get
            {
                if (_gameClientInterface == null)
                {
                    _gameClientInterface = FireballGCI.GetInstance(_logger);
                }
                return _gameClientInterface;
            }
        }
        public FireballSession CurrentSession => _currentSession;

        public string LastActionID => _lastActionID;

        public bool IsConnected => _messenger != null && _messenger.IsConnected;

        public bool IsInit => _currentSession != null
                              && !string.IsNullOrEmpty(_currentSession.ConnectionId)
                              && _messenger.IsInit;

        public bool IsAuth => _currentSession != null
                              && ((!string.IsNullOrEmpty(_currentSession.GameSession) && !string.IsNullOrEmpty(_currentSession.PlayerId))
                              || !string.IsNullOrEmpty(_currentSession.OperatorPlayerSession));
                                
        public bool IsDemo => _currentSession?.GameSession == FireballConfig.DEMO_SESSION
                              || _currentSession?.GameMode?.ToLower() == GameMode.fun.ToString();

        public Action<JackpotUpdateMessage> OnJackpotUpdate { get; set; }
        public Action<string> OnServerConnectionError { get; set; }


        private static readonly object _syncRoot = new object();
        private static Fireball _instance;

        private IFireballLogger _logger = new FireballLogger();
        private INetworkChecker _networkChecker;
        private IMessenger _messenger;
        private ThreadDispatcher _dispatcher;
        private Communicator _communicator = null;
        private Translation _translation = null;
        private FireballMultiplayer _multiplayer = null;
        private FireballGCI _gameClientInterface = null;
        private AuthorizingRequestParams _autorizingParams = null;
        private FireballSession _currentSession;
        private string _customRouterUrl;
        private string _lastActionID;

        private readonly Dictionary<string, string> _pendingRequests = new Dictionary<string, string>();
        private readonly Dictionary<string, JToken> _pendingResponses = new Dictionary<string, JToken>();

        private Action<FireballSession> _onInitSuccess = null;
        private Action<string> _onInitError = null;
        internal Action<string, JToken> _onBroadcastMessageRecieved;

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
            if (_logger == null) _logger = new FireballLogger();
            if (_networkChecker == null) _networkChecker = new NetworkChecker(this, 2.0f);
            if (_dispatcher == null) _dispatcher = new ThreadDispatcher(this);
            if (_communicator == null) _communicator = new Communicator(this, _logger);
            if (_translation == null) _translation = new Translation(_communicator, _logger);

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
            else if (messengerType == MessengerType.BestHTTPv2)
            {
                _messenger = new BestHTTPMessenger(this);
            }
            else
            {
                _messenger = new WebSocketMessenger(_currentSession);
            }

            _messenger.OnMessageReceived += OnMessageReceived;
            _messenger.OnConnectionChange += OnConnectionChange;
            _messenger.OnConnectionError += OnConnectionError;

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

        public void Authorize(AuthRequest authRequest, Action<AuthResponse> onSuccess = null, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1)
        {
            _autorizingParams = new AuthorizingRequestParams(authRequest, onSuccess, onError, timeout, attempts);
            Authorize<AuthRequest, AuthResponse>(authRequest, onSuccess, onError, timeout, attempts);
        }

        public void Authorize<TRequest, TResponse>(TRequest authRequest, Action<TResponse> onSuccess = null, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1) where TRequest : AuthRequest where TResponse : AuthResponse
        {
            SendRequest<TRequest, TResponse>(authRequest,
                response =>
                {
                    _autorizingParams = null;
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
            DemoAuthorize<AuthResponse>(currency, balance, onSuccess, onError);
        }

        public void DemoAuthorize<TResponse>(string currency = FireballConfig.DEFAULT_CURRENCY, long balance = FireballConfig.DEMO_BALANCE, Action<TResponse> onSuccess = null, Action<ErrorResponse> onError = null) where TResponse : AuthResponse, new()
        {
            try
            {
                if (_currentSession == null)
                    _currentSession = new FireballSession();

                _currentSession.GameMode = GameMode.fun.ToString();
                _currentSession.GameSession = FireballConfig.DEMO_SESSION;
                _currentSession.PlayerId = FireballConfig.DEMO_USER_ID;

                var response = new TResponse();
                response.ActionId = FireballTools.GenerateActionID();
                response.MessageTimestamp = FireballTools.GetNowTimestampMilliSeconds();
                response.ConnectionId = _currentSession.ConnectionId;
                response.Balance = balance;
                response.Currency = currency;
                response.GameMode = _currentSession.GameMode;
                response.Environment = _currentSession.Environment;
                response.GameId = _currentSession.GameId;
                response.PlayerId = _currentSession.PlayerId;
                response.GameSession = _currentSession.GameSession;
                response.OperatorId = _currentSession.OperatorId;
                response.OperatorPlayerId = _currentSession.OperatorPlayerId;
                response.OperatorPlayerSession = _currentSession.OperatorPlayerSession;
                response.Extra = _currentSession.Extra;

                onSuccess?.Invoke(response);
            }
            catch (Exception e)
            {
                onError?.Invoke(ErrorResponse.CustomError(null, e.ToString(), 0));
            }
        }

        public void SendPing() =>
            Communicator.SendPOST(URLRouter, new PingRequest(_currentSession));

        private void OnInternetConnection(bool connected)
        {
            if (!IsInit && !connected)
            {
                return;
            }

            if (_messenger is { IsClosed: true })
            {
                _logger.Log($"OnInternetConnection: Connection lost, trying to reconnect...");
                _messenger.Reconnect();
            }
        }

        private void OnConnectionChange(bool isConnected, string connectionId)
        {
            if (isConnected && _currentSession.ConnectionId != connectionId)
            {
                _currentSession.ConnectionId = connectionId;

                _onInitSuccess?.Invoke(_currentSession);
                _onInitSuccess = null;

                if (_autorizingParams != null)
                {
                    _logger.Info($"Continue Auth with new connectionId = {connectionId}");
                    _autorizingParams.Request.ConnectionId = connectionId;
                    Authorize(_autorizingParams.Request, _autorizingParams.OnSuccess, _autorizingParams.OnError, _autorizingParams.Timeout, _autorizingParams.Attempts);
                }
            }
        }

        private void OnConnectionError(string error)
        {
            OnServerConnectionError?.Invoke(error);
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

            _dispatcher?.Dispose();
            _instance = null;
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
            _pendingRequests[request.ActionId] = request.Name;

            if (!IsPendingResponse(request.ActionId))
            {
                Communicator.SendPOST(URLRouter, request, null,
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
                            Communicator.SendPOST(URLRouter, request);
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

                    if (error?.ClientScript?.Value != null)
                    {
                        GameClientInterface.SendIntegrationError(error.ClientScript.Value);
                    }

                    onError?.Invoke(error);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                onError?.Invoke(ErrorResponse.CustomError(request.ActionId, e.ToString()));
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
            //_logger.Log($"Set Pending response actionID = {actionID}");
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
            //_logger.Log($"On Message Received: {json}");
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
                var name = messageObject?[nameof(BaseMessage.Name)] != null ? messageObject[nameof(BaseMessage.Name)].ToString() :
                           messageObject?[nameof(JackpotUpdateMessage.name)] != null ? messageObject[nameof(JackpotUpdateMessage.name)].ToString() :
                           null;

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

                    if (!_pendingRequests.ContainsKey(actionId))
                    {
                        _onBroadcastMessageRecieved?.Invoke(name, messageObject);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"On Message error: can't parse json! Exception: {e.ToString()}");
            }
        }



        public void GetBetTiers(string currency, Action<List<TierData>> onSuccess, Action<string> onError = null)
        {
            var query = new Dictionary<string, string>()
            {
                { "currency", currency }
            };
            Communicator.SendGET(FireballConfig.URL_BET_TIERS, query,
                (json) =>
                {
                    try
                    {
                        _logger.Log($"On BetTiers: success! {json}");
                        var result = JsonConvert.DeserializeObject<BetTiers>(json, new JsonSerializerSettings()
                        {
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        });

                        onSuccess?.Invoke(result.Tiers);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"On BetTiers: Error - can't parse json! Exception: {e.ToString()}");
                        onError?.Invoke(e.Message);
                    }
                },
                onError);
        }

        public void GetTransactionsList(Action<TransactionsList> onSuccess, Action<string> onError = null, int startIndex = 0, bool includeGameStates = true)
        {
            if (!IsAuth)
            {
                _logger.Error($"Transactions: Error - Fireball is not authorized!");
                onError?.Invoke("Fireball is not authorized!");
                return;
            }

            var url = $"{FireballConfig.URL_TRANSACTIONS_HISTORY}/{CurrentSession.ConnectionId}/{includeGameStates}/{startIndex}";
            Communicator.SendGET(url, null,
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
                        _logger.Error($"On Transactions: Error - can't parse json! Exception: {e.ToString()}");
                        onError?.Invoke(e.Message);
                    }
                },
                onError);
        }

        public void GetReplaysList(string shortReplayId, Action<List<Transaction>> onSuccess, Action<string> onError = null)
        {
            var url = $"{FireballConfig.URL_TRANSACTIONS_REPLAY}/{shortReplayId}";
            Communicator.SendGET(url, null,
                (json) =>
                {
                    try
                    {
                        _logger.Log($"On Replays: success! {json}");
                        var result = JsonConvert.DeserializeObject<ReplayList>(json, new JsonSerializerSettings()
                        {
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        });

                        onSuccess?.Invoke(result?.Replays);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"On Replays: Error - can't parse json! Exception: {e.Message}");
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
