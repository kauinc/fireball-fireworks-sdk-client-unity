namespace KAU.FireballSDK
{
    public static class FireballConfig
    {
        public const string URL_FIREBALL_API_DEFAULT = "https://api.fireballserver.com/api/v1.0";
        public const string URL_ROUTER_DEFAULT = "https://cloud.fireballserver.com/router";
        public const string DEFAULT_LANGUAGE_CODE = "en";
        public const string DEFAULT_CURRENCY = "USD";
        public const float DEFAULT_TIMEOUT = 12.0f;
        
        public const int DEMO_BALANCE = 100000;
        public const string DEMO_USER_ID = "demo-player";
        public const string DEMO_SESSION = "demo-session";

        public const Environments DEFAULT_ENVIRONMENT = Environments.development;
        public const GameMode DEFAULT_GAME_MODE = GameMode.fun;
    }
}