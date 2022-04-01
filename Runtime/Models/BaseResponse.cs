using System;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public class BaseResponse : BaseMessage
    {

    }

    [Serializable]
    public class ResponseMessageWrapper<T> where T : BaseResponse
    {
        public string ActionId;
        public string WsMessageId;
        public T Message;
    }
}
