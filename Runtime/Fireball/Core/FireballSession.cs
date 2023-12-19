using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client
{
    [System.Serializable]
    public class FireballSession
    {
        // Fireball data
        public string GameMode = string.Empty;
        public string Environment = string.Empty;
        public string OperatorId = string.Empty;
        public string OperatorPlayerSession = string.Empty;
        public string OperatorPlayerId = string.Empty;
        public string GameId = string.Empty;
        public string PlayerId = string.Empty;
        public string GameSession = string.Empty;
        public string Token = string.Empty;

        // Connection data
        public string Router = string.Empty;
        public string WsServer = string.Empty;
        public string ConnectionToken = string.Empty;
        public string ConnectionId = string.Empty;

        // Personal info
        public string Language = string.Empty;
        public string Currency = string.Empty;
        public string Country = string.Empty;
        public string Gender = string.Empty;

        // Additional custom data
        public string HomeUrl = string.Empty;
        public Dictionary<string, string> Extra = new Dictionary<string, string>();

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}
