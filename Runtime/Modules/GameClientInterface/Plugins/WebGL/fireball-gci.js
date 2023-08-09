// Fireball GCI events
var FIREBALL_EVENTS = {
    FROM_GAME: {
        AUDIO_VOLUME: "game_audio_volume",
        LOADING_COMPLETE: "game_loading_complete",
        LOADING_PROGRESS: "game_loading_progress",
        BET_PLACED: "game_bet_placed",
        BET_RESULT: "game_bet_result",
        BET_UPDATE: "game_bet_update",
        ERROR_MESSAGE: "game_error_message",
        CLOSED: "game_closed",
        INTEGRATION_ERROR: "integration_error",
    },
    TO_GAME: {
        AUDIO_VOLUME: "operator_audio_volume",
        STOP_AUTOPLAY: "operator_stop_autoplay",
        UPDATE_BALANCE: "operator_update_balance",
        VISIBLE_HELP: "operator_visible_help",
        VISIBLE_PAYTABLE: "operator_visible_paytable",
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
        gameAudioVolume: function (volume) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.AUDIO_VOLUME, volume);
        },
        gameLoadingProgress: function (progress) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.LOADING_PROGRESS, progress);
        },
        gameLoadingComplete: function () {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.LOADING_COMPLETE);
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
        gameErrorMessage: function (message) {
            _dispatchEvent(FIREBALL_EVENTS.FROM_GAME.ERROR_MESSAGE, message);
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
        operatorStopAutoplay: function () {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.STOP_AUTOPLAY);
        },
        operatorUpdateBalance: function (balance) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.UPDATE_BALANCE, balance);
        },
        operatorVisibleHelp: function (visible) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.VISIBLE_HELP, visible);
        },
        operatorVisiblePaytable: function (visible) {
            _dispatchEvent(FIREBALL_EVENTS.TO_GAME.VISIBLE_PAYTABLE, visible);
        },

        // Subscribe to any event
        addEventListener: function (event, callback) {
            _fireballEvents.addEventListener(event, callback);
        }
    };
    return functions;
}();