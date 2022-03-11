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
        public string connectionId;
        public long messageTimestamp;
        /* DEPRECATED */
        public string wsToken;

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}