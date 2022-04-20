using System.Collections.Generic;

namespace KAU.FireballSDK.Models
{
    public class AuthResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "session";

        public string Currency { get; set; }
        public int Balance { get; set; }
        public int Coins { get; set; }
        public string LastActionId { get; set; }
        public Dictionary<string, object> GameState { get; set; }
        public Dictionary<string, object> server_side { get; set; }
        public Dictionary<string, object> client_side { get; set; }

        public AuthResponse()
        {
            Name = RESPONSE_NAME;
        }
    }
}