using System;

namespace KAU.FireballSDK.Modules
{
    public interface INetworkChecker
    {
        event Action<bool>  OnNetworkConnectionChanged;
        void StartNetworkCheck();
        void StopNetworkCheck();
    }
}