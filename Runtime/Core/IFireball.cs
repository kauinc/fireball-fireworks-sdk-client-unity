using System;
using System.Collections.Generic;
using KAU.FireballSDK.Models;
using KAU.FireballSDK.Modules;

namespace KAU.FireballSDK
{
    public interface IFireball
    {
        FireballSession CurrentSession { get; set; }
        string LastActionID { get; }
        bool IsInit { get; }
        bool IsAuth { get; }
        bool IsDemo { get; }

        void Init(Action<FireballSession> onInit = null, Action<string> onError = null);
        void Init(string customUrl, Action<FireballSession> onInit = null, Action<string> onError = null, MessengerType messengerType = MessengerType.NativeWebSocket);

        void SendGET(
            string url, 
            Dictionary<string, string> data = null, 
            Action<string> onComplete = null,
            Action<string> onError = null);

        void SendPOST(
            string url,
            IJsonable data,
            Action<string> onComplete = null,
            Action<string> onError = null);

        void SendRequest<TRequest, TResponse>(
            TRequest data,
            Action<TResponse> onSuccess,
            Action<ErrorResponse> onError = null,
            float timeout = 0,
            int attempts = 1)
            where TRequest : BaseRequest where TResponse : BaseResponse;
    }
}