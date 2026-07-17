using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class BaseMessage
    {
        public string Name;
        public string ActionId;
        public string MessageId;
        public int MessageClientDeviceSequence;
        public int MessageServerDeviceSequence;
        public string Environment;
        public string OperatorId;
        public string OperatorPlayerSession;
        public string OperatorPlayerId;
        public string GameId;
        public string PlayerId;
        public string GameSession;
        public string GameMode;
        public string Currency;
        public string ConnectionId;
        public long MessageTimestamp;
        public Dictionary<string, string> Extra;

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}