using System;
using System.Collections.Generic;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public abstract class BaseRequest : BaseModel
    {
        public Dictionary<string, string> extra;

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
                connectionId = session.ConnectionId;
                wsToken = session.WsToken;
                extra = session.Extra;
            }
            
            messageTimestamp = Tools.FireballTools.GetNowTimestampMilliSeconds();
        }
    }
}
