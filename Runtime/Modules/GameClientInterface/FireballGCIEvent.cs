namespace Fireball.Game.Client.Modules
{
    public class FireballGCIEvent
    {
        // events from operator's page
        public const string EVENT_OPERATOR_AUDIO_VOLUME = "operator_audio_volume";
        public const string EVENT_OPERATOR_BET_TURBO = "operator_bet_turbo";
        public const string EVENT_OPERATOR_BET_PLACE = "operator_bet_place"; 
        public const string EVENT_OPERATOR_BET_UPDATE = "operator_bet_update";
        public const string EVENT_OPERATOR_UPDATE_BALANCE = "operator_update_balance";
        public const string EVENT_OPERATOR_STOP_AUTOPLAY = "operator_stop_autoplay";
        public const string EVENT_OPERATOR_VISIBLE_HELP = "operator_visible_help";
        public const string EVENT_OPERATOR_VISIBLE_PAYTABLE = "operator_visible_paytable";
        public const string EVENT_OPERATOR_CONFIRMED_ACTION = "operator_confirmed_action";
        public const string EVENT_OPERATOR_PAUSE_GAME = "operator_pause_game";
        public const string EVENT_OPERATOR_CLOSE_GAME = "operator_close_game";

        // events from game
        public const string EVENT_GAME_LOADING_STARTED = "game_loading_started";
        public const string EVENT_GAME_LOADING_PROGRESS = "game_loading_progress";
        public const string EVENT_GAME_LOADING_COMPLETE = "game_loading_complete";
        public const string EVENT_GAME_READY_PLAY = "game_ready_play";
        public const string EVENT_GAME_AUDIO_VOLUME = "game_audio_volume";
        public const string EVENT_GAME_BET_TURBO = "game_bet_turbo"; 
        public const string EVENT_GAME_BET_PLACED = "game_bet_placed";
        public const string EVENT_GAME_BET_RESULT = "game_bet_result";
        public const string EVENT_GAME_BET_UPDATE = "game_bet_update";
        public const string EVENT_GAME_BALANCE_UPDATED = "game_balance_updated"; 
        public const string EVENT_GAME_JACKPOT_UPDATED = "game_jackpot_updated"; 
        public const string EVENT_GAME_AUTOPLAY_STARTED = "game_autoplay_started"; 
        public const string EVENT_GAME_AUTOPLAY_COMPLETE = "game_autoplay_complete";
        public const string EVENT_GAME_BONUS_FEATURE_STARTED = "game_bonus_feature_started"; 
        public const string EVENT_GAME_BONUS_FEATURE_COMPLETE = "game_bonus_feature_complete"; 
        public const string EVENT_GAME_OPEN_URL = "game_open_url";
        public const string EVENT_GAME_SPLASH_SCREEN_VISIBLE = "game_splash_screen_visible"; 
        public const string EVENT_GAME_CONFIRM_VISIBLE = "game_confirm_visible"; 
        public const string EVENT_GAME_BUY_FEATURE_VISIBLE = "game_buy_feature_visible"; 
        public const string EVENT_GAME_OPEN_DEPOSIT_MENU = "game_open_deposit_menu"; 
        public const string EVENT_GAME_ERROR_MESSAGE = "game_error_message";
        public const string EVENT_GAME_LOCK_INTERACTION = "game_lock_interaction"; 
        public const string EVENT_GAME_CLOSED = "game_closed";

        public const string EVENT_INTEGRATION_ERROR = "integration_error";

        public string name = null;
        public object value = null;

        public FireballGCIEvent(string name, object value = null)
        {
            this.name = name;
            this.value = value;
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
