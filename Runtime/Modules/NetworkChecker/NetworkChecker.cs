using System;
using System.Collections;
using UnityEngine;

namespace KAU.FireballSDK.Modules
{
    public class NetworkChecker : INetworkChecker
    {
        public event Action<bool> OnNetworkConnectionChanged;
        
        private MonoBehaviour _coroutineHandler;
        private Coroutine _checkConnectionCoroutine;
        
        private bool _isNetworkConnected;
        private float _checkInterval;

        public NetworkChecker(MonoBehaviour coroutineHandler, float checkInterval)
        {
            _coroutineHandler = coroutineHandler;
            _checkInterval = checkInterval;
        }

        public void StartNetworkCheck()
        {
            StopNetworkCheck();
                
            _checkConnectionCoroutine = _coroutineHandler.StartCoroutine(NetworkCheckCoroutine());
        }

        public void StopNetworkCheck()
        {
            if(_checkConnectionCoroutine != null)
            {
                _coroutineHandler.StopCoroutine(_checkConnectionCoroutine);
                _checkConnectionCoroutine = null;
            }
        }

        private IEnumerator NetworkCheckCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_checkInterval);
                CheckConnection();
            }
        }
        
        private void CheckConnection()
        {
            bool isNetworkConnected = Application.internetReachability != NetworkReachability.NotReachable;
            
            if (_isNetworkConnected != isNetworkConnected)
            {
                _isNetworkConnected = isNetworkConnected;
                OnNetworkConnectionChanged?.Invoke(_isNetworkConnected);
            }
        }
    }
}