using KAU.FireballSDK.Models;

namespace KAU.FireballSDK.Demo.Requests
{
    public class AuthResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "session";

        public string gameSession;
        public string playerId;
        public string currency;
        public int balance;
        public int coins;
        public string lastActionId;

        public AuthResponse()
        {
            name = RESPONSE_NAME;
        }
    }
}