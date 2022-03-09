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

        void Init(Action<FireballSession> onSuccess = null, Action<string> onError = null);

        void Init(
            string customUrl, 
            Action<FireballSession> onSuccess = null, 
            Action<string> onError = null,
            MessengerType messengerType = MessengerType.NativeWebSocket);

        void SendGET(
            string url, 
            Dictionary<string, string> data = null, 
            Action<string> onSuccess = null,
            Action<string> onError = null);

        void SendPOST(
            string url,
            BaseModel request,
            Action<string> onSuccess = null,
            Action<string> onError = null);

        void SendRequest<TRequest, TResponse>(
            TRequest request,
            Action<TResponse> onSuccess,
            Action<ErrorResponse> onError = null,
            float timeout = 0,
            int attempts = 1)
            where TRequest : BaseRequest where TResponse : BaseResponse;
    }
}