using System.Collections.Generic;

namespace Fireball.Game.Client.Models
{
    public class AuthResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "session";

        public long Balance;
        public long Coins;
        public string LastActionId;
        public Dictionary<string, object> GameState;
        public Dictionary<string, object> server_side;
        public Dictionary<string, object> client_side;

    }
}