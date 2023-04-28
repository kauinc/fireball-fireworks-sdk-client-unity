using System;
using System.Collections.Generic;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
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
        private const string MESSAGE_SEND = "SendMessage";
        private const string MESSAGE_ACKNOWLEDGE = "AcknowledgeMessage";

        private const int RECONNECT_MAX = 5;
        private int _reconnectAttempt;
        private bool _isDisconnecting = false;

        public bool IsInit => _signalR != null;
        public bool IsConnected => _signalR != null && _signalR.State == ConnectionStates.Connected;
        public bool IsClosed => _signalR != null && _signalR.State == ConnectionStates.Closed;

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

            Disconnect();

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

                _signalR = new HubConnection(new Uri(serverUrlFull), new JsonProtocol(new LitJsonEncoder()));
                _signalR.OnConnected += OnConnected;
                _signalR.OnClosed += OnClose;
                _signalR.OnError += OnErrorReceived;
                _signalR.On(MESSAGE_RECEIVE, (string message) => { OnMessage(message); });
                _signalR.StartConnect();
            }
            catch (Exception e)
            {
                _logger.Error($"Exception! {e.Message}");
                onError?.Invoke(e.Message);
            }
        }

        public void Reconnect(Action<string> onConnect = null, Action<string> onError = null)
        {
            if (_reconnectAttempt < RECONNECT_MAX)
            {
                _fireball.InvokeInMainThread(() =>
                {
                    _logger.Log($"Reconnecting... ({_reconnectAttempt + 1}/{RECONNECT_MAX})");
                    _signalR = null;
                    _reconnectAttempt++;
                    Connect(_serverURL, _connectionToken, _onConnectSuccess, _onConnectFail);
                },
                _reconnectAttempt * 0.5f);
            }
            else
            {
                _logger.Error("Can't Reconnect...");
                onError?.Invoke($"Server Unavailable after {_reconnectAttempt} reconnects attempts");
                _reconnectAttempt = 0;

                _onConnectSuccess = null;
                _onConnectFail = null;
            }
        }

        public void Disconnect()
        {
            if (_signalR != null && _signalR.State == ConnectionStates.Connected)
            {
                _logger.Log("Disconnecting...");
                _isDisconnecting = true;
                _signalR.StartClose();
            }
        }


        private void OnConnected(HubConnection connection)
        {
            _reconnectAttempt = 0;
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
            _logger.Log($"Disconnected - {_connectionId} {(_isDisconnecting ? "(Normal)" : "(Abnormal)")}");
            OnConnectionChange?.Invoke(false, _connectionId);

            if (_isDisconnecting)
            {
                _isDisconnecting = false;
            }
            else
            {
                Reconnect(_onConnectSuccess, _onConnectFail);
            }
        }

        private void OnErrorReceived(HubConnection connection, string error)
        {
            _logger.Error($"Error - {error} (state = {_signalR?.State})");
            if (!IsConnected)
            {
                Reconnect((id) =>
                {
                    _logger.Log($"Reconnection successfull after error {error}");
                },
                (error) =>
                {
                    _logger.Error($"Can't reconnect - still: {error}");
                    OnConnectionError?.Invoke(error);
                });
            }
            // _onConnectFail?.Invoke(error);
        }

        private void OnMessage(string message)
        {
            _logger.Log($"{MESSAGE_RECEIVE} - {message}");
            SignalRMessageData data = JsonUtility.FromJson<SignalRMessageData>(message);
            if (data == null)
            {
                _logger.Error("Can't parse message...");
                return;
            }

            SendMessage(MESSAGE_ACKNOWLEDGE, data.WsMessageId);
            OnMessageReceived?.Invoke(message);
        }

        private void SendMessage(string channel, string message)
        {
            if (IsConnected)
            {
                _logger.Log($"Sending {channel} message = {message}");
                _signalR.Send(channel, message);
            }
        }
    }
}
