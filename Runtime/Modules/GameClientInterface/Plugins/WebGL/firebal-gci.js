var FIREBALL_EVENTS = {
    FROM_GAME:{
        AUDIO_VOLUME: "game_audio_volume",
        LOADING_COMPLETE: "game_loading_complete",
        LOADING_PROGRESS: "game_loading_progress",
    },
    TO_GAME:{
        AUDIO_VOLUME: "operator_audio_volume",
        STOP_AUTOPLAY: "operator_stop_autoplay",
        UPDATE_BALANCE: "operator_update_balance",
    },

};

var firebalGCI = {
    OPERATOR_SCRIPT_URL: "https://storage.googleapis.com/fireball-game-clients/games/mount-olympus/test/front-end-script-04.js",
    UNITY_EVENT_RECEIVER: "FireballGCIEventsReceiver",
    UNITY_EVENT_FUNCTION: "ReceiveEvent",

    _init: false,
    _environment: null,
    _operatorId: null,
    _unityInstance: null,
    _fireballEvents: new EventTarget(),

    init: function (unityInstance) {
        try {
            var urlParams = new URLSearchParams(window.location.search);
            this._environment = urlParams.get("environment");
            this._operatorId = urlParams.get("operatorId");
            this._unityInstance = unityInstance;

            console.log("[FIREBALL-GCI-JS] Init...");
            this._addScript(this.OPERATOR_SCRIPT_URL).then(() => {
                console.log("[FIREBALL-GCI-JS] Init: Unity = " + (this._unityInstance ? "enable" : "null"));
                console.log("[FIREBALL-GCI-JS] Init: Environment = " + this._environment);
                console.log("[FIREBALL-GCI-JS] Init: OperatorId = " + this._operatorId);
                this._init = true;
            });
        }
        catch (e) {
            console.error("[FIREBALL-GCI-JS] Exception:", e);
        }
    },

    addEventListenerFromGame: function (eventName, callback) {
        if (!this._init) {
            console.warn("[FIREBALL-GCI-JS] wait for firebalGCI init!");
        }
        this._fireballEvents.addEventListener(eventName, callback);
    },

    sendEventToGame: function (eventName, eventValue) {
        if (this._unityInstance) {
            console.log("[FIREBALL-GCI-JS] sendEventToGame: name = " + eventName + ", value = " + eventValue);
            var eventData = {
                name: eventName,
                value: eventValue,
            };
            this._unityInstance.SendMessage(this.UNITY_EVENT_RECEIVER, this.UNITY_EVENT_FUNCTION, JSON.stringify(eventData));
        }
        else{
            console.error("[FIREBALL-GCI-JS] sendEventToGame error: _unityInstance = null");
        }
    },

    dispatchEventFromGame: function (event) {
        if (!this._init) {
            console.warn("[FIREBALL-GCI-JS] wait for firebalGCI init!");
        }
        console.log("[FIREBALL-GCI-JS] dispatchEventFromGame: event = ", event);
        return this._fireballEvents.dispatchEvent(event);
    },

    _addScript: function (url) {
        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = url;
            script.addEventListener('load', resolve);
            script.addEventListener('error', reject);
            document.body.appendChild(script);
        });
    },
};
