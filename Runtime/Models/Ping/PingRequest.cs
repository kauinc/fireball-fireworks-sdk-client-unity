using System;
using KAU.FireballSDK.Modules;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public class PingRequest : Jsonable
    {
        public string name = "ping";
        public string environment;
        public string operatorId;
        public string gameId;

        public PingRequest(string environment, string operatorId, string gameId)
        {
            this.environment = environment;
            this.operatorId = operatorId;
            this.gameId = gameId;
        }
    }
}
