namespace KAU.FireballSDK.Models
{
    public class BaseRequest : BaseMessage
    {
        public BaseRequest(string name, FireballSession session, string customActionID = null)
        {
            ActionId = string.IsNullOrEmpty(customActionID) ? Tools.FireballTools.GenerateActionID() : customActionID;
            Name = name;
            MessageTimestamp = Tools.FireballTools.GetNowTimestampMilliSeconds();

            if (session != null)
            {
                Environment = session.Environment;
                OperatorId = session.OperatorId;
                OperatorPlayerId = session.OperatorPlayerId;
                OperatorPlayerSession = session.OperatorPlayerSession;
                GameId = session.GameId;
                PlayerId = session.PlayerId;
                GameSession = session.GameSession;
                GameMode = session.GameMode;
                Currency = session.Currency;
                ConnectionId = session.ConnectionId;
                Extra = session.Extra;
            }
        }
    }
}
