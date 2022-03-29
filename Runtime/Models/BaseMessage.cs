using System;
using Newtonsoft.Json;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public class BaseMessage
    {
        public string Name;
        public string ActionId;
        public string Environment;
        public string OperatorId;
        public string GameId;
        public string PlayerId;
        public string GameSession;
        public string ConnectionId;
        public long MessageTimestamp;

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}