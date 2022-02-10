using System;
using KAU.FireballSDK.Modules;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public class MessageDeliveryRequest : Jsonable
    {
        public string logId;
        public string gameSession;

        public MessageDeliveryRequest(string logId, string gameSession)
        {
            this.logId = logId;
            this.gameSession = gameSession;
        }
    }
}
