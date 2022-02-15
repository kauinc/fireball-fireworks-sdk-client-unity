using System;
using Newtonsoft.Json;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public class BaseModel
    {
        public string name;
        public string actionId;
        public string environment;
        public string operatorId;
        public string gameId;
        public string playerId;
        public string gameSession;
        public string wsToken;
        public long messageTimestamp;

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}