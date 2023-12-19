namespace Fireball.Game.Client.Models
{
    public class BalanceRequest : BaseRequest
    {
        public const string REQUEST_NAME = "balance";

        [UnityEngine.Scripting.Preserve]
        public BalanceRequest(FireballSession session, string customActionID = null) : base(REQUEST_NAME, session, customActionID)
        {

        }
    }
}
