namespace KAU.FireballSDK.Models
{
    public class AuthRequest : BaseRequest
    {
        public const string REQUEST_NAME = "authenticate";

        public string Token { get; set; }
        public string Mode { get; set; }

        public AuthRequest(FireballSession session) : base(REQUEST_NAME, session)
        {
            Token = session.Token;
            Mode = session.Mode;
        }
    }
}