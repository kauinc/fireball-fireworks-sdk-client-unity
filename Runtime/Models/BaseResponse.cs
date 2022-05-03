namespace Fireball.Game.Client.Models
{
    public class BaseResponse : BaseMessage
    {

    }

    public class ResponseMessageWrapper<T> where T : BaseResponse
    {
        public string ActionId;
        public string WsMessageId;
        public T Message;
    }
}
