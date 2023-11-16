// Fireball GCI events
var FIREBALL_EVENTS = {
    FROM_GAME: {
        LOADING_STARTED: "game_loading_started",
        LOADING_PROGRESS: "game_loading_progress",
        LOADING_COMPLETE: "game_loading_complete",
        READY_PLAY: "game_ready_play",
        AUDIO_VOLUME: "game_audio_volume",
        BET_TURBO: "game_bet_turbo",
        BET_PLACED: "game_bet_placed",
        BET_RESULT: "game_bet_result",
        BET_UPDATE: "game_bet_update",
        BALANCE_UPDATED: "game_balance_updated",
        JACKPOT_UPDATED: "game_jackpot_updated",
        AUTOPLAY_STARTED: "game_autoplay_started",
        AUTOPLAY_COMPLETE: "game_autoplay_complete",
        BONUS_FEATURE_STARTED: "game_bonus_feature_started",
        BONUS_FEATURE_COMPLETE: "game_bonus_feature_complete",
        OPEN_URL: "game_open_url",
        SPLASH_SCREEN_VISIBLE: "game_splash_screen_visible",
        CONFIRM_VISIBLE: "game_confirm_visible",
        BUY_FEATURE_VISIBLE: "game_buy_feature_visible",
        OPEN_DEPOSIT_MENU: "game_open_deposit_menu",
        ERROR_MESSAGE: "game_error_message",
        LOCK_INTERACTION: "game_lock_interaction",
        CLOSED: "game_closed",
        INTEGRATION_ERROR: "integration_error",
    },
    TO_GAME: {
        AUDIO_VOLUME: "operator_audio_volume",
        BET_TURBO: "operator_bet_turbo",
        BET_PLACE: "operator_bet_place",
        BET_UPDATE: "operator_bet_update",
        UPDATE_BALANCE: "operator_update_balance",
        STOP_AUTOPLAY: "operator_stop_autoplay",
        VISIBLE_HELP: "operator_visible_help",
        VISIBLE_PAYTABLE: "operator_visible_paytable",
        CONFIRMED_ACTION: "operator_confirmed_action",
        PAUSE_GAME: "operator_pause_game",
        CLOSE_GAME: "operator_close_game",
    },
};

// Fireball GCI Front-End script
var fireballGCI = function () {
    const _fireballEvents = new EventTarget();
    var _dispatchEvent = function (eventName, eventValue = null) {
        var customEvent = new CustomEvent(eventName, {
            detail: {
                name: eventName,
                value: eventValue,
            },
        });
        _fireballEvents.dispatchEvent(customEvent);
    };

    var functions = {
        // From Game To Operator Script
        gameLoadingStarted: function () {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.LOADING_STARTED);
        },
        gameLoadingProgress: function (progress) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.LOADING_PROGRESS, progress);
        },
        gameLoadingComplete: function () {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.LOADING_COMPLETE);
        },
        gameReadyToPlay: function () {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.READY_PLAY);
        },
        gameAudioVolume: function (volume) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.AUDIO_VOLUME, volume);
        },
        gameBetTurbo: function (enabled) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.BET_TURBO, enabled);
        },
        gameBetPlaced: function (betValue) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.BET_PLACED, betValue);
        },
        gameBetResult: function (winValue) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.BET_RESULT, winValue);
        },
        gameBetUpdate: function (betValue) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.BET_UPDATE, betValue);
        },
        gameBalanceUpdated: function (balance) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.BALANCE_UPDATED, balance);
        },
        gameJackpotUpdated: function (jackpotDetails) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.JACKPOT_UPDATED, jackpotDetails);
        },
        gameAutoplayStarted: function () {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.AUTOPLAY_STARTED);
        },
        gameAutoplayComplete: function () {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.AUTOPLAY_COMPLETE);
        },
        gameBonusFeatureStarted: function (feature) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.BONUS_FEATURE_STARTED, feature);
        },
        gameBonusFeatureComplete: function (feature) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.BONUS_FEATURE_COMPLETE, feature);
        },
        gameOpenUrl: function (url) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.OPEN_URL, url);
        },
        gameSplashScreenVisible: function (visible) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.SPLASH_SCREEN_VISIBLE, visible);
        },
        gameConfirmVisible: function (visible) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.CONFIRM_VISIBLE, visible);
        },
        gameBuyFeatureVisible: function (visible) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.BUY_FEATURE_VISIBLE, visible);
        },
        gameOpenDepositMenu: function () {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.OPEN_DEPOSIT_MENU);
        },
        gameErrorMessage: function (message) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.ERROR_MESSAGE, message);
        },
        gameLockInteraction: function (lock) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.LOCK_INTERACTION, lock);
        },
        gameClosed: function () {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.CLOSED);
        },
        gameIntegrationError: function (message) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.INTEGRATION_ERROR, message);
        },

        // From Operator To Game Script
        operatorAudioVolume: function (volume) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.AUDIO_VOLUME, volume);
        },
        operatorBetTurbo: function (enabled) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.BET_TURBO, enabled);
        },
        operatorBetPlace: function (betValue) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.BET_PLACE, betValue);
        },
        operatorBetUpdate: function (betValue) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.BET_UPDATE, betValue);
        },
        operatorUpdateBalance: function () {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.UPDATE_BALANCE);
        },
        operatorStopAutoplay: function () {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.STOP_AUTOPLAY);
        },
        operatorVisibleHelp: function (visible) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.VISIBLE_HELP, visible);
        },
        operatorVisiblePaytable: function (visible) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.VISIBLE_PAYTABLE, visible);
        },
        operatorConfirmedAction: function () {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.CONFIRMED_ACTION);
        },
        operatorPauseGame: function (pause) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.PAUSE_GAME, pause);
        },
        operatorCloseGame: function () {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.CLOSE_GAME);
        },

        // Subscribe to any event
        addEventListener: function (event, callback) {
            _fireballEvents.addEventListener(event, callback);
        }
    };
    return functions;
}();