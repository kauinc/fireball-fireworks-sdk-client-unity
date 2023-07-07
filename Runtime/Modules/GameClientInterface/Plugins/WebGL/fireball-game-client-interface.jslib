var UnityGameClientInterface = {
    $unityGCI: {
        OPERATOR_SCRIPT_URL: "https://cloud.fireballserver.com/launcher/game-scripts/",

        init: false,
        environment: null,
        operatorId: null,

        sendEventToUnity: null,
        getPtrFromString: function (str) {
            var bufferSize = lengthBytesUTF8(str) + 1;
            var buffer = _malloc(lengthBytesUTF8(str) + 1);
            stringToUTF8(str, buffer, bufferSize);
            return buffer;
        },
        addScript: function (url) {
            return new Promise((resolve, reject) => {
                const script = document.createElement('script');
                script.src = url;
                script.addEventListener('load', resolve);
                script.addEventListener('error', reject);
                document.body.appendChild(script);
            });
        },
    },

    init: function (eventCallback) {
        try {
            console.log("[FIREBALL-GCI] Init...");
            var urlParams = new URLSearchParams(window.location.search);
            unityGCI.environment = urlParams.get("environment") ?? "production";
            unityGCI.operatorId = urlParams.get("operatorId");
            unityGCI.sendEventToUnity = function (event) {
                var eventJson = JSON.stringify(event.detail);
                var buffer = unityGCI.getPtrFromString(eventJson);
                dynCall_vi(eventCallback, buffer);
            };

            var url = unityGCI.OPERATOR_SCRIPT_URL + unityGCI.environment;
            if (unityGCI.operatorId) {
                url = url + "/" + unityGCI.operatorId;
            }

            console.log("[FIREBALL-GCI] Init: Environment = " + unityGCI.environment);
            console.log("[FIREBALL-GCI] Init: OperatorId = " + unityGCI.operatorId);
            console.log("[FIREBALL-GCI] Init: Add script from URL = " + url);
            unityGCI.addScript(url).then(() => {
                console.log("[FIREBALL-GCI] Init: fireballGCI.js = " + fireballGCI);
                unityGCI.init = true;
                if (fireballGCI) {
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.AUDIO_VOLUME, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.STOP_AUTOPLAY, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.UPDATE_BALANCE, unityGCI.sendEventToUnity);
                }
                else {
                    console.error("[FIREBALL-GCI] fireballGCI = null");
                }
            });
        }
        catch (e) {
            console.error("[FIREBALL-GCI] Exception:", e);
        }
    },

    sendFireballGCIEvent: function (eventData) {
        if(!unityGCI.init){
            console.warn("[FIREBALL-GCI] GCI not inititialized! Skip send event...");
            return;
        }

        var eventJson = UTF8ToString(eventData);
        var unityEvent = JSON.parse(eventJson);
        var eventName = unityEvent.name;
        var eventValue = unityEvent.value;

        if (fireballGCI) {
            switch (eventName) {
                case FIREBALL_EVENTS.FROM_GAME.AUDIO_VOLUME:
                    fireballGCI.gameAudioVolume(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.LOADING_PROGRESS:
                    fireballGCI.gameLoadingProgress(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.LOADING_COMPLETE:
                    fireballGCI.gameLoadingComplete(eventValue);
                    break;
                default:
                    console.warn("[FIREBALL-GCI] Game Event with name = " + eventName + " not found");
            }
        }
        else {
            console.error("[FIREBALL-GCI] fireballGCI = null");
        }
    },
};
autoAddDeps(UnityGameClientInterface, '$unityGCI');
mergeInto(LibraryManager.library, UnityGameClientInterface);