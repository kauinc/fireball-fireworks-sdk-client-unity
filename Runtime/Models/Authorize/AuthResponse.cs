using System.Collections.Generic;
using KAU.FireballSDK.Models;

namespace KAU.FireballSDK.Demo.Requests
{
    public class AuthResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "session";

        public string Currency;
        public int Balance;
        public int Coins;
        public string LastActionId;
        public Dictionary<string, object> GameState;
        public Dictionary<string, object> server_side;
        public Dictionary<string, object> client_side;

        public AuthResponse()
        {
            Name = RESPONSE_NAME;
        }
    }
}