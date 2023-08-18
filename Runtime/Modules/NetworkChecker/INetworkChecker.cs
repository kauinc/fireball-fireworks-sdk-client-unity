using System;

namespace Fireball.Game.Client.Modules
{
    public interface INetworkChecker
    {
        bool IsConnected { get; }
        event Action<bool> OnNetworkConnectionChanged;
        void StartNetworkCheck();
        void StopNetworkCheck();
    }
}