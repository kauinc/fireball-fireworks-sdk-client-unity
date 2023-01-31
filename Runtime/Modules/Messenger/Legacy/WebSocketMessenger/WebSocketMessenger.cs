using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fireball.Game.Client.Tools;
using Fireball.Game.Client.Modules.NativeWebSocket;
using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class WebSocketMessenger : IMessenger
    {
        public bool IsInit => _websocket != null && _websocket.State == WebSocketState.Open;
        public bool IsConnected => _isConnected;
        public bool IsClosed => _websocket == null || _websocket.State == WebSocketState.Closed;

        public Action<bool, string> OnConnectionChange { get; set; }
        public Action<string> OnMessageReceived { get; set; }
        public Action<string> OnConnectionError { get; set; }
        public Action<string> OnError;
        
        private WebSocket _websocket;
        private FireballSession _currentSession;
        private WebSocketMono _webSocketMono = null;
        
        private string _serverURL;
        private string _connectionToken;
        private bool _isDisconnecting;
        private bool _isConnected;
        
        private const int RECONNECT_MAX = 1;
        private int _reconnectAttempt = 0;
        private string _closeMessageJSON;

        private IFireballLogger _logger;
        
        public WebSocketMessenger(FireballSession fireballSession)
        {
            _logger = new FireballLogger("WebSocket");
            _currentSession = fireballSession;
        }

        public void Connect(string server, string connectionToken, Action<string> onConnect = null, Action<string> onError = null)
        {
            ConnectAsync(server, connectionToken, onConnect, onError);
        }

        public void Reconnect(Action<string> onConnect = null, Action<string> onError = null)
        {
            _logger.Log("Reconnecting...");
            _reconnectAttempt++;
            Connect(_serverURL, _currentSession.ConnectionToken, onConnect, onError);
        }

        public void Disconnect()
        {
            DisconnectAsync();
        }

        private async void ConnectAsync(string server, string connectionToken, Action<string> onConnect = null, Action<string> onError = null)
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

            try
            {
                if (_websocket != null && _websocket.State == WebSocketState.Open)
                {
                    await _websocket.Close();
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Exception! {e.Message}");
            }

            try
            {
                string serverUrlFull = FireballTools.FormatUrlAndParams(server, new Dictionary<string, string>()
                {
                    { "EIO", "4" },
                    { "transport", "websocket" },
                    { "connectionToken", connectionToken },
                    { "environment", _currentSession.Environment },
                    { "operatorId", _currentSession.OperatorId },
                    { "gameId", _currentSession.GameId },
                });

                _logger.Log($"Connection opening... server = {serverUrlFull}");

#if !UNITY_WEBGL || UNITY_EDITOR
                if (_webSocketMono == null)
                {
                    _webSocketMono = new GameObject("WebSocketMono").AddComponent<WebSocketMono>();
                }
                _webSocketMono.Init(this);
#endif

                _serverURL = server;
                _closeMessageJSON = new WebSocketClose(connectionToken).ToJson();
                _websocket = new WebSocket(serverUrlFull);
                _websocket.OnOpen +=
                    () =>
                    {
                        if (_isDisconnecting)
                        {
                            return;
                        }

                        OnOpen(connectionToken);
                        onConnect?.Invoke(connectionToken);
                    };
                _websocket.OnError +=
                    (e) =>
                    {
                        if (_isDisconnecting) return;

                        OnParseError(e);
                        onError?.Invoke(e);
                    };
                _websocket.OnClose += OnClose;
                _websocket.OnMessage += OnMessage;

                await _websocket.Connect();
            }
            catch (Exception e)
            {
                _logger.Error($"Exception! {e.Message}");
            }
        }

        private async void DisconnectAsync()
        {
            _isDisconnecting = true;
            if (_websocket != null && _websocket.State == WebSocketState.Open)
            {
                await _websocket.Close(reason: _closeMessageJSON);
            }
        }

        private void OnOpen(string connectionToken)
        {
            _logger.Log("Connection open!");
            
            _reconnectAttempt = 0;
            _connectionToken = connectionToken;
            OnUpdateConnection(true, connectionToken);
        }

        private void OnUpdateConnection(bool isConnected, string connectionToken)
        {
            if (_isConnected != isConnected)
            {
                _isConnected = isConnected;
                OnConnectionChange?.Invoke(_isConnected, connectionToken);
            }
        }

        private void OnClose(WebSocketCloseCode code)
        {
            _logger.Log($"Connection closed! code {code}");
            if (_isDisconnecting) return;

            OnUpdateConnection(false, _connectionToken);

            if (code != WebSocketCloseCode.Normal)
            {
                if (_reconnectAttempt < RECONNECT_MAX)
                {
                    Reconnect();
                }
                else
                {
                    _logger.Error("Can't Reconnect...");
                    _reconnectAttempt = 0;
                    OnError?.Invoke("Reach server error");
                }
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            if(_webSocketMono != null)
            {
                GameObject.Destroy(_webSocketMono);
                _webSocketMono = null;
            }
#endif
        }

        private void OnParseError(string error)
        {
            _logger.Error("Error! " + error);
            OnError?.Invoke(error);
        }
        
        private void OnMessage(byte[] bytes)
        {
            string messageJson = null;
            if (ParseMessage(bytes, out messageJson))
            {
                OnMessageReceived?.Invoke(messageJson);
            }
        }
        
        private bool ParseMessage(byte[] bytes, out string messageJson)
        {
            messageJson = System.Text.Encoding.UTF8.GetString(bytes);
            _logger.Log($"Received OnMessage! message: {messageJson}\n({bytes.Length} bytes)");

            WebSocketData data = JsonUtility.FromJson<WebSocketData>(messageJson);
            if (data == null)
            {
                _logger.Error("Can't parse message...");
                return false;
            }

            SendMessage(new WebSocketAcknowledge(data.wsMessageId).ToJson());
            return true;
        }

        private async void SendMessage(string message, bool log = true)
        {
            if (_websocket.State == WebSocketState.Open)
            {
                if (log) _logger.Log($"Sending message = {message}");
                await _websocket.SendText(message);
            }
        }
        
#if !UNITY_WEBGL || UNITY_EDITOR
        public void OnUpdate()
        {
            if (_websocket != null && IsConnected)
            {
                _websocket.DispatchMessageQueue();
            }
        }
#endif

    }
}
