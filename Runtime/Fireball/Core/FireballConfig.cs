using Fireball.Game.Client.Modules;

namespace Fireball.Game.Client
{
    public static class FireballConfig
    {
        public const string URL_FIREBALL_SERVER = "https://cloud.fireballserver.com";
        public const string URL_FIREBALL_SERVER_API = "https://api.fireballserver.com/api/v2.0";
        public const string URL_ROUTER_DEFAULT = URL_FIREBALL_SERVER + "/router";
        public const string URL_MESENGER_DEFAULT = URL_FIREBALL_SERVER + "/messages/messages";
        public const string URL_TRANSACTIONS_HISTORY = URL_FIREBALL_SERVER + "/sessions/replay/transactions";
        public const string URL_TRANSACTIONS_REPLAY = URL_FIREBALL_SERVER + "/sessions/replay";
        public const string URL_BET_TIERS = URL_FIREBALL_SERVER_API + "/bet-tiers";
        public const string URL_TRANSLATION = URL_FIREBALL_SERVER + "/translations/v1/translate";

        public const string DEFAULT_LANGUAGE_CODE = "en";
        public const string DEFAULT_COUNTRY_CODE = "US";
        public const string DEFAULT_CURRENCY = "USD";
        public const string DEFAULT_COINS_CURRENCY = "COINS";
        public const float DEFAULT_TIMEOUT = 30.0f;
        
        public const int DEMO_BALANCE = 100000;
        public const string DEMO_USER_ID = "demo-player";
        public const string DEMO_SESSION = "demo-session";

        public const Environments DEFAULT_ENVIRONMENT = Environments.development;
        public const GameMode DEFAULT_GAME_MODE = GameMode.fun;

        public static LogLevels LogLevel
        {
            get
            {
                return FireballLogger.LogLevel;
            }
            set
            {
                FireballLogger.LogLevel = value;
                ModuleLogger.LogLevel = (int)value;
            }
        }
    }
}