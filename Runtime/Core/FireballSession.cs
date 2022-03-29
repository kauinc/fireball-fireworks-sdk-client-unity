using System.Collections.Generic;

namespace KAU.FireballSDK
{
    [System.Serializable]
    public class FireballSession
    {
        // Fireball data
        public string Mode = string.Empty;
        public string Environment = string.Empty;
        public string OperatorId = string.Empty;
        public string GameId = string.Empty;
        public string PlayerId = string.Empty;
        public string GameSession = string.Empty;
        public string Token = string.Empty;
        public string ConnectionId = string.Empty;

        /* DEPRECATED */
        public string ConnectionToken = string.Empty;
        public string WsToken = string.Empty;

        // Personal info
        public string Language = string.Empty;
        public string Currency = string.Empty;
        public string Country = string.Empty;
        public string Gender = string.Empty;

        // Additional custom data
        public string WsServer = string.Empty;
        public string Router = string.Empty;
        public string HomeUrl = string.Empty;
        public Dictionary<string, string> Extra = new Dictionary<string, string>();
    }
}
