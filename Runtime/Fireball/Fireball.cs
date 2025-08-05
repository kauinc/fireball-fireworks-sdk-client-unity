using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                    _multiplayer = new FireballMultiplayer(this, _logger);
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
                    _gameClientInterface = FireballGCI.GetInstance();
                }
                return _gameClientInterface;
            }
        }
        public FireballSession CurrentSession => _currentSession;
        public INetworkChecker Network => _networkChecker;

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
        public Action<ServerMessage> OnServerMessageReceived { get; set; }
        public Action<string> OnServerConnectionError { get; set; }

        private static readonly string RESEND_FAILED_REQUEST_ACTION = "resend-failed-request";
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
        private string _lastFailedActionID;

        private readonly Dictionary<string, string> _pendingRequests = new Dictionary<string, string>();
        private readonly Dictionary<string, JToken> _pendingResponses = new Dictionary<string, JToken>();

        private Action<FireballSession> _onInitSuccess = null;
        private Action<string> _onInitError = null;
        internal Action<ServerMessage> _onBroadcastMessageReceived;

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
            Initialize(playerData != null ? playerData.GetSession() : URLData.ParseSessionFromURL(), onSuccess, onError);
        }

        private void Initialize(FireballSession customSession, Action<FireballSession> onSuccess = null, Action<string> onError = null)
        {
            if (_logger == null) _logger = new FireballLogger();
            if (_networkChecker == null) _networkChecker = new NetworkChecker(this, 0.5f);
            if (_dispatcher == null) _dispatcher = new ThreadDispatcher(this);
            if (_communicator == null) _communicator = new Communicator(this, _logger);
            if (_translation == null) _translation = new Translation(_communicator, _logger);

            _onInitSuccess = onSuccess;
            _onInitError = onError;

            _logger.Log("Init...");
            _currentSession = customSession;
            _currentSession.ConnectionToken = FireballTools.GenerateConnectionToken();

            _customRouterUrl = _currentSession.Router;

            // Websocket messenger module init
            _messenger = new BestHTTPMessenger(this);

            _messenger.OnMessageReceived += OnMessageReceived;
            _messenger.OnConnectionChange += OnMessengerConnectionChange;
            _messenger.OnConnectionError += OnMessengerConnectionError;

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
                    _onInitError = null;
                },
                (error) =>
                {
                    _logger.Error($"OnInit: Error! {error}");
                    _onInitError?.Invoke(error);

                    _onInitSuccess = null;
                    _onInitError = null;
                });

            CurrencyHelper.SetSession(_currentSession);
            WebBrowser.OnTabVisibility(OnTabVisibility);
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

                    // copy extra params
                    if (response.Extra != null)
                    {
                        if (_currentSession.Extra == null)
                        {
                            _currentSession.Extra = new Dictionary<string, string>();
                        }

                        foreach (var key in response.Extra.Keys)
                        {
                            _currentSession.Extra[key] = response.Extra[key];
                        }
                    }

                    // fixing currency changing
                    if (response.Currency != null && _currentSession.Currency != response.Currency)
                    {
                        _logger.Warning($"Currency changed: {_currentSession.Currency} -> {response.Currency}");
                        _currentSession.Currency = response.Currency;
                        if (_currentSession.Extra.ContainsKey(URLData.PARAM_NAME_CURRENCY))
                        {
                            _currentSession.Extra[URLData.PARAM_NAME_CURRENCY] = response.Currency;
                        }
                    }

                    if (response.Multiplier != null)
                    {
                        _currentSession.Multiplier = response.Multiplier;
                    }
                    
                    CurrencyHelper.SetSession(_currentSession);

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

        public void GetBalance(BalanceRequest request, Action<BalanceResponse> onSuccess = null, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1)
        {
            SendRequest<BalanceRequest, BalanceResponse>(request, onSuccess, onError, timeout, attempts);
        }

        public void SendPing() =>
            Communicator.SendPOST(URLRouter, new PingRequest(_currentSession));

        private void OnInternetConnection(bool connected)
        {
            if (!IsInit && !connected)
            {
                return;
            }

            if (!connected)
            {
                OnServerConnectionError?.Invoke("Connection lost...");
            }

            if (_messenger is { IsClosed: true })
            {
                _logger.Log($"OnInternetConnection: Connection lost, trying to reconnect...");
                _messenger.Reconnect();
            }
        }

        private void OnMessengerConnectionChange(bool isConnected, string connectionId)
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

        private void OnMessengerConnectionError(string error)
        {
            if (!_networkChecker.IsConnected)
            {
                _logger.Error($"Messenger Error: {error}");
            }

            OnServerConnectionError?.Invoke(error);
        }
        
        private void OnDestroy()
        {
            WebBrowser.RemoveTabVisibility(OnTabVisibility);
            
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
            where TResponse : BaseResponse
        {
            StartCoroutine(SendRequestCoroutine(request, onSuccess, onError, timeout, attempts));
        }

        private IEnumerator SendRequestCoroutine<TRequest, TResponse>(TRequest request, Action<TResponse> onSuccess, Action<ErrorResponse> onError, float timeout, int attemptsCount)
            where TRequest : BaseRequest
            where TResponse : BaseResponse
        {
            if (_messenger == null || !_messenger.IsConnected || !_networkChecker.IsConnected)
            {
                _logger.Error("Can't send request! Fail to connect to server");
                onError?.Invoke(ErrorResponse.CustomError(request.ActionId, ErrorResponse.NO_CONNECTION_REASON));
                yield break;
            }

            //CheckAndClearPendingResponses();
            _lastFailedActionID = null;
            _lastActionID = request.ActionId;

            float timePassed = 0;
            int attemptsLeft = attemptsCount - 1;
            _pendingRequests[request.ActionId] = request.Name;
            
            var connectionId = _messenger.ConnectionId;
            if (request.ConnectionId != connectionId)
            {
                _logger.Log($"Connection Id different from messengers! Changing Connection Id: {request.ConnectionId} -> {connectionId}");
                request.ConnectionId = connectionId;
                _currentSession.ConnectionId = connectionId;
            }

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
                    _logger.Info($"Message - {response.Name} - Received (ActionId: {response?.ActionId}, GameSession: {response?.GameSession})" +
                        $"\nMessage: {response.ToJson()}" +
                        $"\nTime passed: {timePassed:F1} sec, Attempts: {attemptsCount}");

                    _lastFailedActionID = null;
                    onSuccess?.Invoke(response);
                }
                else
                {
                    _lastFailedActionID = request.ActionId;
                    var error = responseObject.ToObject<ErrorResponse>();
                    _logger.Error($"Message - {error.Name} - Error (ActionId: {error?.ActionId}, GameSession: {error?.GameSession})" +
                        $"\nError: {error.ToJson()}" +
                        $"\nTime passed: {timePassed:F1} sec, Attempts: {attemptsCount}");

                    if (error?.Error?.ClientScript?.Value != null)
                    {
                        GameClientInterface.SendIntegrationError(error.Error?.ClientScript.Value);
                    }

                    onError?.Invoke(error);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                _lastFailedActionID = request.ActionId;
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


            while (_lastFailedActionID != null && _lastFailedActionID != RESEND_FAILED_REQUEST_ACTION)
            {
                //_logger.Warning("Wait for resend failed request...");
                yield return null;
            }

            if (_lastFailedActionID == RESEND_FAILED_REQUEST_ACTION)
            {
                //_logger.Warning("Resending last failed request...");
                _lastFailedActionID = null;
                SendRequest(request, onSuccess, onError, timeout, attemptsCount);
            }
            else
            {
                //_logger.Warning("Skipped failed request...");
            }
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
                        _onBroadcastMessageReceived?.Invoke(new ServerMessage(actionId, name, messageObject));
                    }

                    OnServerMessageReceived?.Invoke(new ServerMessage(actionId, name, messageObject));
                }
            }
            catch (Exception e)
            {
                _logger.Error($"On Message error: can't parse json! Exception: {e.ToString()}");
            }
        }

        public void ResendFailedRequest()
        {
            if (_lastFailedActionID != null && _lastFailedActionID != RESEND_FAILED_REQUEST_ACTION)
            {
                _lastFailedActionID = RESEND_FAILED_REQUEST_ACTION;
            }
            else
            {
                _logger.Warning("No failed requests found...");
            }
        }

        public void GetBetTiers(string currency, Action<List<BetTier>> onSuccess, Action<string> onError = null)
        {
            var query = new Dictionary<string, string>()
            {
                { "currencyIsoCode", currency }
            };
            Communicator.SendGET(FireballConfig.URL_BET_TIERS, query,
                (json) =>
                {
                    try
                    {
                        _logger.Log($"On BetTiers: success! {json}");
                        var result = JsonConvert.DeserializeObject<ServerBetTiersData>(json, new JsonSerializerSettings()
                        {
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        });

                        var betTierList = result.Data.Select(b => b.ToBetTier()).ToList();
                        betTierList = betTierList.OrderBy(b => b.ValueDefault).ToList();
                        betTierList.ForEach(b => b.Tier = betTierList.IndexOf(b) + 1);

                        _logger.Log($"On BetTiers: sorted - {JsonConvert.SerializeObject(betTierList)}");

                        onSuccess?.Invoke(betTierList);
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

        public void GoHomePage()
        {
            if (string.IsNullOrEmpty(CurrentSession?.HomeUrl))
            { 
                _logger.Warning("Home param were not provided in game URL! Trying close game by sending event to operators page...");
                GameClientInterface.SendGameClosed();
            }
            else
            {
                WebBrowser.SetLocation(CurrentSession.HomeUrl);
            }
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
        
        private void OnTabVisibility(bool visible)
        {
            if (visible)
            {
                if (!_messenger.IsConnected)
                {
                    _logger.Error("Disconnected due inactive!");
                    OnServerConnectionError?.Invoke("Disconnected due inactive!");
                }
            }
        }
    } 
}
