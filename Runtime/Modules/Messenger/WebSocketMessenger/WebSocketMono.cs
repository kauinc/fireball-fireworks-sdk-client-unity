using UnityEngine;

namespace KAU.FireballSDK.Modules
{
    public class WebSocketMono : MonoBehaviour
    {
        private WebSocketMessenger _webSocket;

        public void Init(WebSocketMessenger socket)
        {
            _webSocket = socket;
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        public void Update()
        {
            if (_webSocket != null && _webSocket.IsInit)
            {
                _webSocket.OnUpdate();
            }
        }
#endif
    }
}