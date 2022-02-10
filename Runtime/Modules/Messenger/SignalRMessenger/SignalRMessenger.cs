using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KAU.FireballSDK.Tools;
using KAU.FireballSDK.Modules.SignalRPlugin;

namespace KAU.FireballSDK.Modules
{
    public enum SignalRState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    [System.Serializable]
    public class SignalRMessageData
    {
        public string name;
        public string wsMessageId;
    }

    public class SignalRMessenger : IMessenger
    {
        public bool IsInit => _signalR != null && _state == SignalRState.Open;
        public bool IsConnected
        {
            get => _isConnected.Value;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnConnectionChange?.Invoke(_isConnected.Value);
                }
            }
        }
        public bool IsClosed => _signalR == null || _state == SignalRState.Closed;

        public Action<bool> OnConnectionChange { get; set; }
        public Action<string> OnMessageReceived { get; set; }
        public Action<string> OnError;

        private const string MESSAGE_RECEIVE = "ReceiveMessage";
        private const string MESSAGE_SEND = "SendMessage";
        private const string MESSAGE_ACKNOWLEDGE = "AcknowledgeMessage";

        private const int RECONNECT_MAX = 1;
        private int _reconectAttempt = 0;

        private IFireballLogger _fireballLogger = null;
        private SignalR _signalR = null;
        private FireballSession _currentSession;
        private string _serverURL = null;

        private bool? _isConnected = null;
        private SignalRState _state = SignalRState.Closed;

        public SignalRMessenger(FireballSession fireballSession)
        {
            _currentSession = fireballSession;
            _fireballLogger = new FireballLogger();
        }

        public void Connect(string server, string wsToken, Action OnConnect = null, Action<string> OnError = null)
        {
            if (string.IsNullOrEmpty(server))
            {
                _fireballLogger.LogError("Can't connect! Server = null");
                OnError?.Invoke("Can't connect! Server = null");
                return;
            }
            if (string.IsNullOrEmpty(wsToken))
            {
                _fireballLogger.LogError("Can't connect! wsToken = null");
                OnError?.Invoke("Can't connect! wsToken = null");
                return;
            }

            Disconnect();

            try
            {
                var _serverUrlFull = FireballTools.FormatUrlAndParams(server, new Dictionary<string, string>()
                {
                    { "EIO", "4" },
                    { "transport", "websocket" },
                    { "wsToken", wsToken },
                    { "environment", _currentSession.Environment },
                    { "operatorId", _currentSession.OperatorId },
                    { "gameId", _currentSession.GameId },
                });

                _fireballLogger.Log($"SignalR: Connecting... server = {_serverUrlFull}");
                _serverURL = server;

                _signalR = new SignalR();
                _signalR.Init(_serverUrlFull);

                _signalR.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
                {
                    _fireballLogger.Log($"SignalR: Connected - {e.ConnectionId}");
                    OnOpen();
                    OnConnect?.Invoke();
                };

                _signalR.ConnectionClosed += OnClose;
                _signalR.On(MESSAGE_RECEIVE, (string message) =>
                {
                    OnMessage(message);
                });

                _state = SignalRState.Connecting;
                _signalR.Connect();
            }
            catch (Exception e)
            {
                _fireballLogger.LogError($"Exception! {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }
        public void Reconnect()
        {
            _fireballLogger.Log("SignalR: Reconnecting...");
            _signalR = null;
            _reconectAttempt++;
            Connect(_serverURL, _currentSession.WsToken);
        }
        public void Disconnect()
        {
            if (_signalR != null && _state == SignalRState.Open)
            {
                _fireballLogger.Log("SignalR: Disconnectting...");
                _state = SignalRState.Closing;
                _signalR.Stop();
            }
        }

        private void OnOpen()
        {
            _state = SignalRState.Open;
            _reconectAttempt = 0;
            IsConnected = true;
        }
        private void OnClose(object sender, ConnectionEventArgs e)
        {
            _fireballLogger.Log($"SignalR: Disconnected - {e.ConnectionId} {(_state == SignalRState.Closing ? "(Normal)" : "(Abnormal)")}");
            IsConnected = false;
            if (_state == SignalRState.Closing)
            {
                _state = SignalRState.Closed;
            }
            else
            {
                _state = SignalRState.Closed;
                if (_reconectAttempt < RECONNECT_MAX)
                {
                    Reconnect();
                }
                else
                {
                    _fireballLogger.LogError("SignalR: Can't Reconnect...");
                    _reconectAttempt = 0;
                    OnError?.Invoke("Reach server error");
                }
            }
        }
        private void OnMessage(string message)
        {
            _fireballLogger.Log($"SignalR: {MESSAGE_RECEIVE} - {message}");
            SignalRMessageData data = JsonUtility.FromJson<SignalRMessageData>(message);
            if (data == null)
            {
                _fireballLogger.LogError("SignalR: Can't parse message...");
                return;
            }
            SendMessage(MESSAGE_ACKNOWLEDGE, data.wsMessageId);
            OnMessageReceived?.Invoke(message);
        }
        private void SendMessage(string channel, string message)
        {
            if (IsConnected)
            {
                _fireballLogger.Log($"SignalR: Sending message = {message} to channel = {channel}");
                _signalR.Invoke(channel, message);
            }
        }
    }
}
