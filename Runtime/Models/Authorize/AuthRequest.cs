using KAU.FireballSDK;
using KAU.FireballSDK.Models;

namespace KAU.FireballSDK.Models
{
    public class AuthRequest : BaseRequest
    {
        public const string REQUEST_NAME = "authenticate";

        public string Token;
        public string Mode;
        
        public AuthRequest(FireballSession session) : base(REQUEST_NAME, session)
        {
            Token = session.Token;
            Mode = session.Mode;
        }
    }
}