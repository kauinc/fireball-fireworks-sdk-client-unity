using System;
using System.Collections.Generic;
using Best.SignalR;
using Best.SignalR.Encoders;
using Fireball.Game.Client.Tools;
using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class BestHTTPMessenger : IMessenger
    {
        private IFireballLogger _logger;
        private IFireball _fireball;
        private HubConnection _signalR;

        private string _serverURL;
        private string _connectionId;
        private string _connectionToken;

        private const string MESSAGE_RECEIVE = "ReceiveMessage";
        private const string MESSAGE_ACKNOWLEDGE = "AcknowledgeMessage";

        private const int RECONNECT_MAX = 7;
        private int _reconnectAttempt;
        private bool _isDisconnecting = false;
        private bool _isReconnecting = false;
        private bool _isConnecting = false;

        public bool IsInit => _signalR != null;
        public bool IsConnected => _signalR != null && _signalR.State == ConnectionStates.Connected;
        public bool IsClosed => _signalR == null || _signalR.State == ConnectionStates.Closed;
        public string ConnectionId => _signalR?.NegotiationResult?.ConnectionId;

        private Action<string> _onConnectSuccess = null;
        private Action<string> _onConnectFail = null;

        public Action<bool, string> OnConnectionChange { get; set; }
        public Action<string> OnMessageReceived { get; set; }
        public Action<string> OnConnectionError { get; set; }

        public BestHTTPMessenger(IFireball fireball)
        {
            _fireball = fireball;
            _logger = new FireballLogger("SignalR");
        }

        public void Connect(string server, string connectionToken, Action<string> onConnect = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(server))
            {
                _logger.Error("Can't connect! Server = null");
                onError?.Invoke("Can't connect! Server = null");
                return;
            }
            if (string.IsNullOrEmpty(connectionToken))
            {
                _logger.Error("Can't connect! ConnectionToken = null");
                onError?.Invoke("Can't connect! ConnectionToken = null");
                return;
            }
            
            UnsubscribeCurrentSignalR();
            
            if (_signalR != null && _signalR.State == ConnectionStates.Connected)
            {
                _logger.Log("Closing old connection before reconnect...");
                _isDisconnecting = true;
                _signalR.StartClose();
            }

            _isConnecting = true;
            try
            {
                string serverUrlFull = FireballTools.FormatUrlAndParams(server, new Dictionary<string, string>()
                {
                    {"EIO", "4"},
                    {"transport", "websocket"},
                    {"connectionToken", connectionToken},
                    {"environment", _fireball.CurrentSession.Environment},
                    {"operatorId", _fireball.CurrentSession.OperatorId},
                    {"gameId", _fireball.CurrentSession.GameId},
                });

                _logger.Log($"Connecting... server = {serverUrlFull}");
                _serverURL = server;
                _connectionToken = connectionToken;
                _onConnectSuccess = onConnect;
                _onConnectFail = onError;

                _signalR = new HubConnection(new Uri(serverUrlFull), new JsonProtocol(new LitJsonEncoder()), new HubOptions()
                {
                    PingInterval = TimeSpan.FromSeconds(5),
                    PingTimeoutInterval = TimeSpan.FromSeconds(25),
                    ConnectTimeout = TimeSpan.FromSeconds(10),
                });
                _signalR.OnConnected += OnConnected;
                _signalR.OnClosed += OnClose;
                _signalR.OnError += OnErrorReceived;
                _signalR.On(MESSAGE_RECEIVE, (string message) => OnMessage(message));
                _signalR.StartConnect();
            }
            catch (Exception e)
            {
                _logger.Error($"Exception! {e.ToString()}");
                _isReconnecting = false;
                _isConnecting = false;
                onError?.Invoke(e.Message);
            }
        }

        public void Reconnect(Action<string> onConnect = null, Action<string> onError = null)
        {
            if (_isReconnecting || _isConnecting)
            {
                _logger.Log($"Reconnect: skipping — isReconnecting={_isReconnecting}, isConnecting={_isConnecting}.");
                return;
            }
            if (!_fireball.Network.IsConnected)
            {
                _logger.Warning("Reconnect: no network — will retry when network restores.");
                return;
            }
            ScheduleReconnect(onConnect, onError);
        }

        public void ForceReconnect(Action<string> onConnect = null, Action<string> onError = null)
        {
            if (_isReconnecting || _isConnecting)
            {
                _logger.Log($"ForceReconnect: skipping — isReconnecting={_isReconnecting}, isConnecting={_isConnecting}.");
                return;
            }
            _logger.Log($"ForceReconnect: resetting backoff (was attempt {_reconnectAttempt}).");
            _reconnectAttempt = 0;
            ScheduleReconnect(onConnect, onError);
        }

        public void Disconnect()
        {
            UnsubscribeCurrentSignalR();
            if (_signalR != null && _signalR.State == ConnectionStates.Connected)
            {
                _logger.Log("Disconnecting...");
                _isDisconnecting = true;
                _signalR.StartClose();
            }
        }

        private void ScheduleReconnect(Action<string> onConnect, Action<string> onError)
        {
            if (_reconnectAttempt < RECONNECT_MAX)
            {
                _isReconnecting = true;
                float delay = (float)Math.Pow(2, _reconnectAttempt);
                _logger.Log($"Reconnecting in {delay:F1}s... ({_reconnectAttempt + 1}/{RECONNECT_MAX})");

                var capturedUrl = _serverURL;
                var capturedToken = _connectionToken;
                _fireball.InvokeInMainThread(() =>
                {
                    _isReconnecting = false;
                    _reconnectAttempt++;
                    Connect(capturedUrl, capturedToken,
                        onConnect ?? _onConnectSuccess,
                        onError ?? _onConnectFail);
                }, delay);
            }
            else
            {
                _logger.Error($"Can't Reconnect after {RECONNECT_MAX} attempts.");
                _isReconnecting = false;
                _reconnectAttempt = 0;

                var msg = $"Server Unavailable after {RECONNECT_MAX} reconnect attempts";
                if (onError != null)
                    onError.Invoke(msg);
                else if (_onConnectFail != null)
                {
                    _onConnectFail.Invoke(msg);
                    _onConnectSuccess = null;
                    _onConnectFail = null;
                }
                else
                    OnConnectionError?.Invoke(msg);
            }
        }

        private void UnsubscribeCurrentSignalR()
        {
            if (_signalR == null) return;
            _signalR.OnConnected -= OnConnected;
            _signalR.OnClosed   -= OnClose;
            _signalR.OnError    -= OnErrorReceived;
            _signalR.Remove(MESSAGE_RECEIVE);
        }
        
        private void OnConnected(HubConnection connection)
        {
            _reconnectAttempt = 0;
            _isReconnecting = false;
            _isConnecting = false;
            _isDisconnecting = false;
            _connectionId = connection?.NegotiationResult?.ConnectionId;

            _logger.Log($"Connected - {_connectionId}");
            _onConnectSuccess?.Invoke(_connectionId);
            OnConnectionChange?.Invoke(true, _connectionId);

            _onConnectSuccess = null;
            _onConnectFail = null;
        }

        private void OnClose(HubConnection connection)
        {
            _isConnecting = false;
            _logger.Log($"Disconnected - {_connectionId} {(_isDisconnecting ? "(Normal)" : "(Abnormal)")}");
            OnConnectionChange?.Invoke(false, _connectionId);

            if (_isDisconnecting)
                _isDisconnecting = false;
            else
                Reconnect(_onConnectSuccess, _onConnectFail);
        }

        private void OnErrorReceived(HubConnection connection, string error)
        {
            _isConnecting = false;
            _logger.Error($"Error - {error} (state = {_signalR?.State})");
            if (!IsConnected)
            {
                _isReconnecting = false;
                Reconnect(
                    onConnect: (id) => _logger.Log($"Reconnected after error: {error}"),
                    onError: (err) =>
                    {
                        _logger.Error($"Can't reconnect after error: {err}");
                        if (_onConnectFail != null)
                        {
                            _onConnectFail.Invoke(err);
                            _onConnectSuccess = null;
                            _onConnectFail = null;
                        }
                        else
                            OnConnectionError?.Invoke(err);
                    });
            }
        }

        private void OnMessage(string message)
        {
            SignalRMessageData data = Newtonsoft.Json.JsonConvert.DeserializeObject<SignalRMessageData>(message);
            if (data == null)
            {
                OnMessageReceived?.Invoke(message);
                return;
            }

            if (!string.IsNullOrEmpty(data.WsMessageId))
            {
                SendAcknowledge(data.WsMessageId);
            }

            OnMessageReceived?.Invoke(message);
        }

        private void SendAcknowledge(string wsMessageId)
        {
            try
            {
                if (_signalR == null || !IsConnected)
                {
                    _logger.Error($"SendAcknowledge: not connected (state={_signalR?.State}), skip {wsMessageId}");
                    return;
                }

                _signalR.Send(MESSAGE_ACKNOWLEDGE, wsMessageId)
                    .OnError(ex => _logger.Error($"AcknowledgeMessage failed: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.Error($"SendAcknowledge exception: {ex}");
            }
        }
    }
}