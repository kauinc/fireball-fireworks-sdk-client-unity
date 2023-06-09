var FIREBALL_GCI_EVENT = {
    STOP_AUTOSPINS: "StopAutospins",
    AUDIO_TOGGLE: "AudioToggle",
    UPDATE_BALANCE: "UpdateBalance",
    LOADING_COMPLETE: "LoadingComplete",
    LOADING_PROGRESS: "LoadingProgress",
};

var firebalGCI = {
    OPERATOR_SCRIPT_URL: "https://storage.googleapis.com/fireball-game-clients/games/mount-olympus/test/front-end-script-03.js",
    UNITY_EVENT_RECEIVER: "FireballGCIEventsReceiver",

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
            console.log("[FIREBALL-GCI-JS] sendEventToGame: " + eventName + " = " + eventValue);
            this._unityInstance.SendMessage(this.UNITY_EVENT_RECEIVER, eventName, eventValue);
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
