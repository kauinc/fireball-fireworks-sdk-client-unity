using System.Collections.Generic;
using KAU.FireballSDK.Modules;
using Newtonsoft.Json;

namespace KAU.FireballSDK.Models
{
    [System.Serializable]
    public abstract class BaseRequest : Jsonable
    {
        public string actionId;
        public string name;
        public string environment;
        public string operatorId;
        public string gameId;
        public string playerId;
        public string gameSession;
        public string wsToken;
        public Dictionary<string, string> extra;
        public long messageTimestamp;

        protected BaseRequest(string name, FireballSession session, string customActionID = null)
        {
            actionId = string.IsNullOrEmpty(customActionID) ? Tools.FireballTools.GenerateActionID() : customActionID;
            this.name = name;
            if (session != null)
            {
                environment = session.Environment;
                operatorId = session.OperatorId;
                gameId = session.GameId;
                playerId = session.PlayerId;
                gameSession = session.GameSession;
                wsToken = session.WsToken;
                extra = session.Extra;
            }
            
            messageTimestamp = Tools.FireballTools.GetNowTimestampMilliSeconds();
        }
    }
}
