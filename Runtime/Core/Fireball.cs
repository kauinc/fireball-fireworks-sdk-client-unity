using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using KAU.FireballSDK.Models;
using KAU.FireballSDK.Modules;
using KAU.FireballSDK.Tools;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace KAU.FireballSDK
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

        public FireballSession CurrentSession { get; set; }
        public string LastActionID => _lastActionID;

        public bool IsInit => CurrentSession != null
                              && !string.IsNullOrEmpty(CurrentSession.WsToken)
                              && _messenger.IsInit;

        public bool IsAuth => CurrentSession != null
                              && !string.IsNullOrEmpty(CurrentSession.GameSession)
                              && !string.IsNullOrEmpty(CurrentSession.PlayerId);

        public bool IsDemo => CurrentSession.GameSession.Equals(FireballConfig.DEMO_SESSION);

        private static readonly object _syncRoot = new object();
        private static Fireball _instance;

        private const float UPDATE_TIME = 0.1f;
        private string _customRouterUrl;

        private string _lastActionID;

        private IMessenger _messenger;
        private IFireballLogger _fireballLogger;
        private INetworkChecker _networkChecker;

        private readonly Dictionary<string, string> _pendingRequests = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _pendingResponses = new Dictionary<string, string>();


        private string URLRouter =>
            !string.IsNullOrEmpty(_customRouterUrl) ? _customRouterUrl : FireballConfig.URL_ROUTER_DEFAULT;

        public void Init(Action<FireballSession> onSuccess = null, Action<string> onError = null)
        {
            Init(URLData.ParseSessionFromURL(), onSuccess, onError);
        }

        public void Init(
            string customUrl, 
            Action<FireballSession> onSuccess = null, 
            Action<string> onError = null,
            MessengerType messengerType = MessengerType.NativeWebSocket)
        {
            Init(URLData.ParseSessionFromURL(customUrl), onSuccess, onError, messengerType);
        }

        private void Init(
            FireballSession customSession, 
            Action<FireballSession> onSuccess = null,
            Action<string> onError = null, 
            MessengerType messengerType = MessengerType.NativeWebSocket)
        {
            _fireballLogger = new FireballLogger();
            _networkChecker = new NetworkChecker(this, 2.0f);

            _fireballLogger.Log("Init...");
            CurrentSession = customSession;
            CurrentSession.ConnectionToken = FireballTools.GenerateConnectionToken();

            _customRouterUrl = CurrentSession.Router;

            // Websocket module init
            if (messengerType == MessengerType.SignalR)
            {
                _messenger = new SignalRMessenger(CurrentSession);
            }
            else
            {
                _messenger = new WebSocketMessenger(CurrentSession);
            }

            _messenger.OnMessageReceived += OnMessageReceived;
            _messenger.OnConnectionChange += OnConnectionChange;

            // Send ping to warm up Fireball system
            SendPing();

            // Start check network connection
            _networkChecker.StartNetworkCheck();
            _networkChecker.OnNetworkConnectionChanged += OnInternetConnection;

            // Connect to App Messages WebSocket server
            _messenger.Connect(CurrentSession.WsServer, CurrentSession.ConnectionToken,
                (connectionId) =>
                {
                    _fireballLogger.Log("OnInit: Success!");
                    CurrentSession.ConnectionId = connectionId;
                    onSuccess?.Invoke(CurrentSession);
                },
                (error) =>
                {
                    _fireballLogger.LogError($"OnInit: Error! {error}");
                    onError?.Invoke(error);
                });
        }

        public void SendPing() =>
            SendPOST(URLRouter, new PingRequest(CurrentSession));

        private void OnInternetConnection(bool connected)
        {
            if (!IsInit && !connected)
            {
                return;
            }

            _fireballLogger.Log($"On Connection change: connected = {connected}");

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

        public void SendGET(
            string url,
            Dictionary<string, string> data = null,
            Action<string> onSuccess = null,
            Action<string> onError = null) =>
            StartCoroutine(SendGETCoroutine(url, data, onSuccess, onError));

        private IEnumerator SendGETCoroutine(
            string url,
            Dictionary<string, string> data,
            Action<string> onSuccess,
            Action<string> onError)
        {
            url = FireballTools.FormatUrlAndParams(url, data);

            _fireballLogger.Log($"SEND GET: url = {url}");
            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                _fireballLogger.LogError($"Error: {www.error}");
                onError?.Invoke(www.error);
            }
            else
            {
                _fireballLogger.Log($"Complete: {www.downloadHandler.text} ({www.responseCode})");
                onSuccess?.Invoke(www.downloadHandler.text);
            }
        }

        public void SendPOST(
            string url,
            BaseMessage request,
            Action<string> onSuccess = null,
            Action<string> onError = null) =>
            StartCoroutine(SendPOSTCoroutine(url, request, onSuccess, onError));

        private IEnumerator SendPOSTCoroutine(
            string url,
            BaseMessage request,
            Action<string> onSuccess,
            Action<string> onError)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(request.ToJson());

            _fireballLogger.Log($"SEND POST: url = {url}");
            _fireballLogger.Log($"SEND POST: data = {request.ToJson()}");

            using UnityWebRequest www = UnityWebRequest.Post(url, request.ToJson());
            www.uploadHandler = new UploadHandlerRaw(bytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                _fireballLogger.LogError($"Error: {www.error}");
                onError?.Invoke(www.error);
            }
            else
            {
                _fireballLogger.Log($"Complete: {www.downloadHandler.text} ({www.responseCode})");
                onSuccess?.Invoke(www.downloadHandler.text);
            }
        }

        public void SendRequest<TRequest, TResponse>(
            TRequest request,
            Action<TResponse> onSuccess,
            Action<ErrorResponse> onError = null,
            float timeout = FireballConfig.DEFAULT_TIMEOUT,
            int attempts = 1)
            where TRequest : BaseRequest where TResponse : BaseResponse =>
            StartCoroutine(SendRequestCoroutine(request, onSuccess, onError, timeout, attempts));

        private IEnumerator SendRequestCoroutine<TRequest, TResponse>(
            TRequest request,
            Action<TResponse> onSuccess,
            Action<ErrorResponse> onError,
            float timeout,
            int attemptsCount)
            where TRequest : BaseRequest where TResponse : BaseResponse
        {
            CheckAndClearPendingResponses();

            _lastActionID = request.ActionId;

            float timePassed = 0;
            int attemptsLeft = attemptsCount - 1;
            _pendingRequests.Add(request.ActionId, request.Name);

            SendPOST(URLRouter, request, null,
                errorReason => { onError?.Invoke(new ErrorResponse() {reason = errorReason}); });

            while (!IsPendingResponse(request.ActionId))
            {
                if (timeout > 0 && timePassed >= timeout)
                {
                    if (attemptsLeft > 0)
                    {
                        attemptsLeft--;
                        timePassed = 0;
                        _fireballLogger.Log($"Timeout! Try next attempt... ({attemptsCount - attemptsLeft})");
                        SendPOST(URLRouter, request);
                    }
                    else
                    {
                        _fireballLogger.LogError($"Timeout for message {request.Name} {request.ActionId}! " +
                                                 $"Time passed: {timePassed} sec! " +
                                                 $"Attempts = {attemptsCount - attemptsLeft}");

                        ErrorResponse error = new ErrorResponse()
                        {
                            ActionId = request.ActionId,
                            Name = ErrorResponse.TYPE_TIMEOUT,
                            type = ErrorResponse.TYPE_TIMEOUT,
                            reason = ErrorResponse.MakeReason(string.Format(ErrorResponse.MESSAGE_TIMEOUT, timeout))
                        };
                        AddPendingResponse(request.ActionId, error.ToJson());
                    }
                }
                else
                {
                    yield return new WaitForSeconds(UPDATE_TIME);
                    timePassed += UPDATE_TIME;
                }
            }

            _fireballLogger.Log($"Message received {request.Name} {request.ActionId}! " +
                                $"Time passed: {timePassed:F1} sec! " +
                                $"Attempts = {attemptsCount}");

            string responseString = GetPendingResponse(request.ActionId);
            ResponseMessageWrapper<TResponse> response = JsonConvert.DeserializeObject<ResponseMessageWrapper<TResponse>>(responseString);

            if (response != null)
            {
                onSuccess?.Invoke(response.Message);
            }
            else
            {
                onError?.Invoke(ErrorResponse.ParseError(responseString));
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

        private string GetPendingResponse(string actionID)
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

        private void AddPendingResponse(string actionID, string response)
        {
            _fireballLogger.Log($"Set Pending response actionID = {actionID}");

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
                _fireballLogger.Log($"Clearing Pending response = {string.Join(",", responses)}");
                foreach (string actionID in responses)
                {
                    _pendingResponses.Remove(actionID);
                }
            }
        }

        private void OnMessageReceived(string json)
        {
            _fireballLogger.Log($"On Message Received: data = {json}");

            ResponseMessageWrapper<BaseResponse> data = JsonConvert.DeserializeObject<ResponseMessageWrapper<BaseResponse>>(json);
            if (data == null)
            {
                _fireballLogger.LogError("On Message error: can't parse json!");
                return;
            }

            if (string.IsNullOrEmpty(data.ActionId))
            {
                _fireballLogger.LogError("On Message error: actionID == null!");
                AddPendingResponse(_lastActionID, json);
            }
            else
            {
                AddPendingResponse(data.ActionId, json);
            }
        }

        private void OnConnectionChange(bool isConnected, string connectionId)
        {
            if (isConnected)
            {
                CurrentSession.ConnectionId = connectionId;
            }
        }
    }
}