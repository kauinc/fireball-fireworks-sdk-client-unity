namespace KAU.FireballSDK.Models
{
    public class BaseResponse : BaseMessage
    {

    }

    public class ResponseMessageWrapper<T> where T : BaseResponse
    {
        public string ActionId { get; set; }
        public string WsMessageId { get; set; }
        public T Message { get; set; }
    }
}
