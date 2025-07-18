using System;

namespace Fireball.Game.Client.Modules
{
    public interface IMessenger
    {
        bool IsInit { get; }
        bool IsConnected { get; }
        bool IsClosed { get; }
        string ConnectionId { get; }

        Action<bool, string> OnConnectionChange { get; set; }
        Action<string> OnMessageReceived { get; set; }
        Action<string> OnConnectionError { get; set; }

        void Connect(string server, string connectionToken, Action<string> onConnect = null, Action<string> onError = null);
        void Reconnect(Action<string> onConnect = null, Action<string> onError = null);
        void Disconnect();
    }
}
