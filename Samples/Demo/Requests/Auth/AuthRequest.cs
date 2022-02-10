using KAU.FireballSDK;
using KAU.FireballSDK.Models;

namespace KAU.FireballSDK.Demo.Requests
{
    public class AuthRequest : BaseRequest
    {
        public const string REQUEST_NAME = "authenticate";

        public string token;
        public string mode;
        
        public AuthRequest(string token, string gameMode, FireballSession session, string customActionID = null) 
            : base(REQUEST_NAME, session, customActionID)
        {
            this.token = token;
            mode = gameMode;
        }
    }
}