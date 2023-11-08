namespace Fireball.Game.Client.Modules
{
    public class FireballGCIEvent
    {
        public const string EVENT_OPERATOR_AUDIO_VOLUME = "operator_audio_volume";
        public const string EVENT_OPERATOR_BET_TURBO = "operator_bet_turbo";// added 
        public const string EVENT_OPERATOR_BET_PLACE = "operator_bet_place";// added 
        public const string EVENT_OPERATOR_BET_UPDATE = "operator_bet_update";// added 
        public const string EVENT_OPERATOR_UPDATE_BALANCE = "operator_update_balance";
        public const string EVENT_OPERATOR_STOP_AUTOPLAY = "operator_stop_autoplay";
        public const string EVENT_OPERATOR_VISIBLE_HELP = "operator_visible_help";
        public const string EVENT_OPERATOR_VISIBLE_PAYTABLE = "operator_visible_paytable";
        public const string EVENT_OPERATOR_PAUSE_GAME = "operator_pause_game";// added 
        // TODO questioned
        // - confirmedContinue
        // - initialized
        // - exitingGame

        public const string EVENT_GAME_LOADING_STARTED = "game_loading_started"; // added
        public const string EVENT_GAME_LOADING_PROGRESS = "game_loading_progress";
        public const string EVENT_GAME_LOADING_COMPLETE = "game_loading_complete";
        public const string EVENT_GAME_READY_PLAY = "game_ready_play"; // added
        public const string EVENT_GAME_AUDIO_VOLUME = "game_audio_volume";
        public const string EVENT_GAME_BET_TURBO = "game_bet_turbo";// added 
        public const string EVENT_GAME_BET_PLACED = "game_bet_placed";
        public const string EVENT_GAME_BET_RESULT = "game_bet_result";
        public const string EVENT_GAME_BET_UPDATE = "game_bet_update";
        public const string EVENT_GAME_BALANCE_UPDATED = "game_balance_updated";// added 
        public const string EVENT_GAME_AUTOPLAY_STARTED = "game_autoplay_started";// added 
        public const string EVENT_GAME_AUTOPLAY_COMPLETE = "game_autoplay_complete";// added 
        public const string EVENT_GAME_BONUS_FEATURE_STARTED = "game_bonus_feature_started";// added 
        public const string EVENT_GAME_BONUS_FEATURE_COMPLETE = "game_bonus_feature_complete";// added 
        public const string EVENT_GAME_OPEN_URL = "game_open_url";// added 
        public const string EVENT_GAME_ERROR_MESSAGE = "game_error_message";
        public const string EVENT_GAME_CLOSED = "game_closed";
        // TODO questioned
        // - jackpotResolved
        // - showHistory
        // - reportUIState
        // - openQuickDeposit


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
