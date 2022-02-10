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

        public bool IsAuth => CurrentSession != null
                              && !string.IsNullOrEmpty(CurrentSession.GameSession)
                              && !string.IsNullOrEmpty(CurrentSession.PlayerId);

        public bool IsDemo => CurrentSession.GameSession.Equals(FireballConfig.DEMO_SESSION);

        private static readonly object _syncRoot = new object();
        private static Fireball _instance;
        
        private const float TIMEOUT = 12.0f;
        private const float UPDATE_TIME = 0.1f;
        private string _customRouterUrl;

        private string _lastActionID;

        private IMessenger _messenger;
        private IFireballLogger _fireballLogger;
        private INetworkChecker _networkChecker;
        
        private readonly Dictionary<string, string> _pendingRequests = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _pendingResponses = new Dictionary<string, string>();

        private bool IsInit => CurrentSession != null
                               && !string.IsNullOrEmpty(CurrentSession.WsToken)
                               && _messenger.IsInit;
        
        private string URLRouter =>
            !string.IsNullOrEmpty(_customRouterUrl) ? _customRouterUrl : FireballConfig.URL_ROUTER_DEFAULT;

        public void Init(Action<FireballSession> onInit = null, Action<string> onError = null)
        {
            Init(URLData.ParseSessionFromURL(), onInit, onError);
        }

        private void Init(FireballSession customSession, Action<FireballSession> onInit = null, Action<string> onError = null)
        {
            _fireballLogger = new FireballLogger();
            _networkChecker = new NetworkChecker(this, 2.0f);
            
            _fireballLogger.Log("Init...");
            CurrentSession = customSession;
            _customRouterUrl = CurrentSession.Router;

            // Websocket module init
            _messenger = new SignalRMessenger(CurrentSession);//new WebSocketMessaging(CurrentSession);
            _messenger.OnMessageReceived += OnMessageReceived;

            // Send ping to warm up Fireball system
            SendPing(CurrentSession.Environment, CurrentSession.OperatorId, CurrentSession.GameId);

            // Start check network connection
            _networkChecker.StartNetworkCheck();
            _networkChecker.OnNetworkConnectionChanged += OnInternetConnection;

            // Connect to App Messages WebSocket server
            _messenger.Connect(CurrentSession.WsServer, CurrentSession.WsToken,
                () =>
                {
                    _fireballLogger.Log("OnInit: Success!");
                    onInit?.Invoke(CurrentSession);
                },
                (error) =>
                {
                    _fireballLogger.LogError($"OnInit: Error! {error}");
                    onError?.Invoke(error);
                });
        }
        
        private void SendPing(string environment, string operatorId, string gameId)
        {
            SendPOST(URLRouter, new PingRequest(environment, operatorId, gameId));
        }

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
            Action<string> onComplete = null, 
            Action<string> onError = null) =>
            StartCoroutine(SendGETCoroutine(url, data, onComplete, onError));

        private IEnumerator SendGETCoroutine(
            string url, 
            Dictionary<string, string> data = null,
            Action<string> onComplete = null, 
            Action<string> onError = null)
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
                onComplete?.Invoke(www.downloadHandler.text);
            }
        }

        public void SendPOST(
            string url, 
            IJsonable data, 
            Action<string> onComplete = null, 
            Action<string> onError = null) =>
            StartCoroutine(SendPOSTCoroutine(url, data, onComplete, onError));

        private IEnumerator SendPOSTCoroutine(
            string url, 
            IJsonable data, 
            Action<string> onComplete = null, 
            Action<string> onError = null)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data.ToJson());

            _fireballLogger.Log($"SEND POST: url = {url}");
            _fireballLogger.Log($"SEND POST: data = {data.ToJson()}");
            
            using UnityWebRequest www = UnityWebRequest.Post(url, data.ToJson());
            www.uploadHandler = new UploadHandlerRaw(bytes);
            www.downloadHandler = new DownloadHandlerBuffer(); 
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                _fireballLogger.LogError($"Error: {www.error}");
                onError?.Invoke(www.error);
            }
            else
            {
                _fireballLogger.Log($"Complete: {www.downloadHandler.text} ({www.responseCode})");
                onComplete?.Invoke(www.downloadHandler.text);
            }
        }

        public void SendRequest<TRequest, TResponse>(
            TRequest data,
            Action<TResponse> onSuccess, 
            Action<ErrorResponse> onError = null, 
            float timeout = 0, 
            int attempts = 1)
            where TRequest : BaseRequest where TResponse : BaseResponse =>
            StartCoroutine(SendRequestCoroutine(data, onSuccess, onError, timeout, attempts));
        
        private IEnumerator SendRequestCoroutine<TRequest, TResponse>(
            TRequest data,
            Action<TResponse> onSuccess, 
            Action<ErrorResponse> onError = null, 
            float timeout = 0, 
            int attemptsCount = 1)
            where TRequest : BaseRequest where TResponse : BaseResponse
        {
            CheckAndClearPendingResponses();

            _lastActionID = data.actionId;

            float timePassed = 0;
            int attemptsLeft = attemptsCount - 1;
            _pendingRequests.Add(data.actionId, data.name);
            
            SendPOST(URLRouter, data, null, errorReason =>
            {
                onError?.Invoke(new ErrorResponse() {reason = errorReason});
            });

            while (!IsPendingResponse(data.actionId))
            {
                if (timeout > 0 && timePassed >= timeout)
                {
                    if (attemptsLeft > 0)
                    {
                        attemptsLeft--;
                        timePassed = 0;
                        _fireballLogger.Log($"Timeout! Try next attempt... ({attemptsCount - attemptsLeft})");
                        SendPOST(URLRouter, data);
                    }
                    else
                    {
                        _fireballLogger.LogError(
                            $"Timeout for message {data.name} {data.actionId}! Time passed: {timePassed} sec! Attempts = {attemptsCount - attemptsLeft}");
                        
                        ErrorResponse error = new ErrorResponse()
                        {
                            actionId = data.actionId,
                            name = ErrorResponse.TYPE_TIMEOUT,
                            type = ErrorResponse.TYPE_TIMEOUT,
                            reason = ErrorResponse.MakeReason(string.Format(ErrorResponse.MESSAGE_TIMEOUT, timeout))
                        };
                        AddPendingResponse(data.actionId, error.ToJson());
                    }
                }
                else
                {
                    yield return new WaitForSeconds(UPDATE_TIME);
                    timePassed += UPDATE_TIME;
                }
            }

            _fireballLogger.Log(
                $"Message received {data.name} {data.actionId}! time passed: {timePassed:F1} sec! Attempts = {attemptsCount}");

            string responseString = GetPendingResponse(data.actionId);
            TResponse response = JsonConvert.DeserializeObject<TResponse>(responseString);

            if (response != null)
            {
                onSuccess?.Invoke(response);
            }
            else
            {
                onError?.Invoke(ErrorResponse.ParseError(responseString));
            }

            if (_pendingRequests.ContainsKey(data.actionId))
            {
                _pendingRequests.Remove(data.actionId);
            }
            
            if (_pendingResponses.ContainsKey(data.actionId))
            {
                _pendingResponses.Remove(data.actionId);
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
            Debug.Log($"On Message Received: data = {json}");
            
            BaseResponse data = JsonUtility.FromJson<BaseResponse>(json);
            if (data == null)
            {
                _fireballLogger.LogError("On Message error: can't parse json!");
                return;
            }

            if (string.IsNullOrEmpty(data.actionId))
            {
                _fireballLogger.LogError("On Message error: actionID == null!");
                AddPendingResponse(_lastActionID, json);
            }
            else
            {
                AddPendingResponse(data.actionId, json);
            }
        }
    }
}