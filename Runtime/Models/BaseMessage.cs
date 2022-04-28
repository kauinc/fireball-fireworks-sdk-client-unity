using System.Collections.Generic;
using Newtonsoft.Json;

namespace KAU.FireballSDK.Models
{
    public abstract class BaseMessage
    {
        public string Name { get; set; }
        public string ActionId { get; set; }
        public string Environment { get; set; }
        public string OperatorId { get; set; }
        public string OperatorPlayerSession { get; set; }
        public string OperatorPlayerId { get; set; }
        public string GameId { get; set; }
        public string PlayerId { get; set; }
        public string GameSession { get; set; }
        public string GameMode { get; set; }
        public string Currency { get; set; }
        public string ConnectionId { get; set; }
        public long MessageTimestamp { get; set; }
        public Dictionary<string, string> Extra { get; set; }

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}