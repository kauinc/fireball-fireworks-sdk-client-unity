using System;
using System.Collections;
using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class NetworkChecker : INetworkChecker
    {
        public bool IsConnected => _isNetworkConnected != null ? _isNetworkConnected.Value : false;

        public event Action<bool> OnNetworkConnectionChanged;

        private ModuleLogger _logger;
        private MonoBehaviour _coroutineHandler;
        private Coroutine _checkConnectionCoroutine;
        
        private bool? _isNetworkConnected;
        private float _checkInterval;

        public NetworkChecker(MonoBehaviour coroutineHandler, float checkInterval)
        {
            _coroutineHandler = coroutineHandler;
            _checkInterval = checkInterval;
            _logger = new ModuleLogger("Network");
        }

        public void StartNetworkCheck()
        {
            StopNetworkCheck();
                
            _checkConnectionCoroutine = _coroutineHandler.StartCoroutine(NetworkCheckCoroutine());
        }

        public void StopNetworkCheck()
        {
            if (_checkConnectionCoroutine != null)
            {
                _coroutineHandler.StopCoroutine(_checkConnectionCoroutine);
                _checkConnectionCoroutine = null;
            }
        }

        private IEnumerator NetworkCheckCoroutine()
        {
            while (true)
            {
                CheckConnection();
                yield return new WaitForSeconds(_checkInterval);
            }
        }
        
        private void CheckConnection()
        {
            bool isNetworkConnected = GetInternetConnectionSimple();
            if (_isNetworkConnected == null)
            {
                _logger.Log($"is connected = {isNetworkConnected}");
                _isNetworkConnected = isNetworkConnected;
            }
            else if (_isNetworkConnected != isNetworkConnected)
            {
                _logger.Log($"Connection Changed: connected = {isNetworkConnected}");
                _isNetworkConnected = isNetworkConnected;
                OnNetworkConnectionChanged?.Invoke(_isNetworkConnected.Value);
            }
        }

        private bool GetInternetConnectionSimple()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebBrowser.IsOnline;
#else
            return Application.internetReachability != NetworkReachability.NotReachable;
#endif
        }
    }
}