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

            console.log("[GCI-JS] Init...");
            this._addScript(this.OPERATOR_SCRIPT_URL).then(() => {
                console.log("[GCI-JS] Init: Unity = " + (this._unityInstance ? "enable" : "null"));
                console.log("[GCI-JS] Init: Environment = " + this._environment);
                console.log("[GCI-JS] Init: OperatorId = " + this._operatorId);
                this._init = true;
            });
        }
        catch (e) {
            console.error("[GCI-JS] Exception:", e);
        }
    },

    addEventListenerFromGame: function (eventName, callback) {
        if (!this._init) {
            console.warn("[GCI-JS] not init!");
        }
        this._fireballEvents.addEventListener(eventName, callback);
    },

    sendEventToGame: function (eventName, eventValue) {
        if (!this._init) {
            console.warn("[GCI-JS] not init!");
        }
        console.log("[GCI-JS] sendEventToGame: " + eventName + " = " + eventValue);
        this._unityInstance.SendMessage(this.UNITY_EVENT_RECEIVER, eventName, eventValue);
    },

    dispatchEventFromGame: function (event) {
        if (!this._init) {
            console.warn("[GCI-JS] not init!");
        }
        console.log("[GCI-JS] dispatchEventFromGame: event = ", event);
        var result = this._fireballEvents.dispatchEvent(event);
    },

    _addScript: function (url) {
        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = url;
            script.addEventListener('load', resolve);
            script.addEventListener('error', reject);
            document.head.appendChild(script);
        });
    },
};
