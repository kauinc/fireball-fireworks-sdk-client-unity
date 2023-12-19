using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fireball.Game.Client.Models
{
    public class ServerMessage
    {
        public readonly string ActionId;
        public readonly string Name;

        private readonly JToken _message;

        public ServerMessage(string actionId, string name, JToken message)
        {
            ActionId = actionId;
            Name = name;
            _message = message;
        }

        public T GetMessage<T>() where T : BaseMessage
        {
            try
            {
                return _message.ToObject<T>();

            }
            catch(Exception e)
            {

            }
            return null;
        }
    }
}
