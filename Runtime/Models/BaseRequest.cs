namespace KAU.FireballSDK.Models
{
    public class BaseRequest : BaseMessage
    {
        public BaseRequest(string name, FireballSession session, string customActionID = null)
        {
            ActionId = string.IsNullOrEmpty(customActionID) ? Tools.FireballTools.GenerateActionID() : customActionID;
            this.Name = name;
            
            if (session != null)
            {
                Environment = session.Environment;
                OperatorId = session.OperatorId;
                GameId = session.GameId;
                PlayerId = session.PlayerId;
                GameSession = session.GameSession;
                ConnectionId = session.ConnectionId;
                Extra = session.Extra;
            }
            
            MessageTimestamp = Tools.FireballTools.GetNowTimestampMilliSeconds();
        }
    }
}
