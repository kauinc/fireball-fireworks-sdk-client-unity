using Fireball.Game.Client.Modules;

namespace Fireball.Game.Client
{
    public static class FireballConfig
    {
        public const string URL_FIREBALL_SERVER = "https://cloud.fireballserver.com";
        public const string URL_ROUTER_DEFAULT = URL_FIREBALL_SERVER + "/router";
        public const string URL_MESENGER_DEFAULT = URL_FIREBALL_SERVER + "/messages/messages";
        public const string URL_REPLAY_TRANSACTION = URL_FIREBALL_SERVER + "/sessions/replay/transactions";

        public const string DEFAULT_LANGUAGE_CODE = "en";
        public const string DEFAULT_COUNTRY_CODE = "US";
        public const string DEFAULT_CURRENCY = "USD";
        public const float DEFAULT_TIMEOUT = 12.0f;
        
        public const int DEMO_BALANCE = 100000;
        public const string DEMO_USER_ID = "demo-player";
        public const string DEMO_SESSION = "demo-session";

        public const Environments DEFAULT_ENVIRONMENT = Environments.development;
        public const GameMode DEFAULT_GAME_MODE = GameMode.fun;

        public static LogLevels LogLevel = LogLevels.Information;
    }
}