namespace Fireball.Game.Client.Models
{
    public class AuthRequest : BaseRequest
    {
        public const string REQUEST_NAME = "authenticate";

        public string Token;

        [UnityEngine.Scripting.Preserve]
        public AuthRequest(FireballSession session, string customActionID = null) : base(REQUEST_NAME, session, customActionID)
        {
            Token = session.Token;
        }
    }
}