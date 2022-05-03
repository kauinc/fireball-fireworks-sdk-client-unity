using System;

namespace Fireball.Game.Client.Modules
{
    public interface INetworkChecker
    {
        event Action<bool>  OnNetworkConnectionChanged;
        void StartNetworkCheck();
        void StopNetworkCheck();
    }
}