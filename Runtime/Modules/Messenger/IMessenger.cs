using System;

namespace KAU.FireballSDK.Modules
{
    public interface IMessenger
    {
        bool IsInit { get; }
        bool IsConnected { get; }
        bool IsClosed { get; }

        Action<bool> OnConnectionChange { get; set; }
        Action<string> OnMessageReceived { get; set; }

        void Connect(string server, string wsToken, Action onConnect = null, Action<string> onError = null);
        void Reconnect();
        void Disconnect();
    }
}
