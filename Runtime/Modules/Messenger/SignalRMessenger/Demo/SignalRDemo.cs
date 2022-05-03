using UnityEngine;

namespace Fireball.Game.Client.Modules.SignalRPlugin.Demo
{
    public class SignalRDemo : MonoBehaviour
    {
        private SignalR _signalR = null;

        public string ServerURL = "https://messages-net-dot-fireball-231121.ey.r.appspot.com/messages";
        public string MessagesReceiveChannel = "ReceiveMessage";
        public string MessagesSendChannel = "SendMessage";

        public void Connect()
        {
            _signalR = new SignalR();
            _signalR.Init(ServerURL);

            _signalR.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
                Debug.Log($"Connected: {e.ConnectionId}");

            _signalR.ConnectionClosed += (object sender, ConnectionEventArgs e) =>
                Debug.Log($"Disconnected: {e.ConnectionId}");

            _signalR.On(MessagesReceiveChannel, (string message) =>
                Debug.Log($"{MessagesReceiveChannel}: {message}"));

            _signalR.Connect();
        }

        public void SendRMessage(string message)
        {
            Debug.Log($"{MessagesSendChannel}: {message}");
            _signalR?.Invoke(MessagesSendChannel, message);
        }

        public void Disconnect()
        {
            _signalR?.Stop();
        }
    }
}