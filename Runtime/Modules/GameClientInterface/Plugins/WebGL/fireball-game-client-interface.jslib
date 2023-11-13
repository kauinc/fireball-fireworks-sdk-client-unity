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
            return new Promise(function (resolve, reject) {
                const script = document.createElement('script');
                script.src = url;
                script.addEventListener('load', resolve);
                script.addEventListener('error', reject);
                document.body.appendChild(script);
            });
        },
        getScriptURL: function (urlParams) {
            this.environment = urlParams.get("environment") ? urlParams.get("environment") : "production";
            this.operatorId = urlParams.get("operatorId");
            var url = this.OPERATOR_SCRIPT_URL + this.environment;
            if (this.operatorId) {
                url = url + "/" + this.operatorId;
            }
            return url;
        },
    },
    isInit: function () {
        return unityGCI.init;
    },
    init: function (eventCallback) {
        try {
            console.log("[FIREBALL-GCI] Init...");
            var urlParams = new URLSearchParams(window.location.search);
            var url = unityGCI.getScriptURL(urlParams);

            console.log("[FIREBALL-GCI] Init: Environment = " + unityGCI.environment);
            console.log("[FIREBALL-GCI] Init: OperatorId = " + unityGCI.operatorId);
            console.log("[FIREBALL-GCI] Init: Add script from URL = " + url);
            unityGCI.addScript(url).then(function () {
                unityGCI.init = true;
                if (fireballGCI) {
                    console.log("[FIREBALL-GCI] Init: fireballGCI.js = " + fireballGCI);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.AUDIO_VOLUME, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.BET_TURBO, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.BET_PLACE, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.BET_UPDATE, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.UPDATE_BALANCE, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.STOP_AUTOPLAY, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.VISIBLE_HELP, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.VISIBLE_PAYTABLE, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.CONFIRMED_ACTION, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.PAUSE_GAME, unityGCI.sendEventToUnity);
                    fireballGCI.addEventListener(FIREBALL_EVENTS.TO_GAME.CLOSE_GAME, unityGCI.sendEventToUnity);
                }
                else {
                    console.error("[FIREBALL-GCI] fireballGCI = null");
                }
            });
            unityGCI.sendEventToUnity = function (event) {
                var eventJson = JSON.stringify(event.detail);
                var buffer = unityGCI.getPtrFromString(eventJson);
                dynCall_vi(eventCallback, buffer);
            };
        }
        catch (e) {
            console.error("[FIREBALL-GCI] Exception:", e);
        }
    },
    sendFireballGCIEvent: function (eventData) {
        if (!unityGCI.init) {
            console.warn("[FIREBALL-GCI] GCI not inititialized! Skip send event...");
            return;
        }

        var eventJson = UTF8ToString(eventData);
        var unityEvent = JSON.parse(eventJson);
        var eventName = unityEvent.name;
        var eventValue = unityEvent.value;

        if (fireballGCI) {
            switch (eventName) {
                case FIREBALL_EVENTS.FROM_GAME.LOADING_STARTED:
                    fireballGCI.gameLoadingStarted();
                    break;
                case FIREBALL_EVENTS.FROM_GAME.LOADING_PROGRESS:
                    fireballGCI.gameLoadingProgress(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.LOADING_COMPLETE:
                    fireballGCI.gameLoadingComplete();
                    break;
                case FIREBALL_EVENTS.FROM_GAME.READY_PLAY:
                    fireballGCI.gameReadyToPlay();
                    break;
                case FIREBALL_EVENTS.FROM_GAME.AUDIO_VOLUME:
                    fireballGCI.gameAudioVolume(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.BET_TURBO:
                    fireballGCI.gameBetTurbo(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.BET_PLACED:
                    fireballGCI.gameBetPlaced(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.BET_RESULT:
                    fireballGCI.gameBetResult(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.BET_UPDATE:
                    fireballGCI.gameBetUpdate(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.BALANCE_UPDATED:
                    fireballGCI.gameBalanceUpdated(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.JACKPOT_UPDATED:
                    fireballGCI.gameJackpotUpdated(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.AUTOPLAY_STARTED:
                    fireballGCI.gameAutoplayStarted();
                    break;
                case FIREBALL_EVENTS.FROM_GAME.AUTOPLAY_COMPLETE:
                    fireballGCI.gameAutoplayComplete();
                    break;
                case FIREBALL_EVENTS.FROM_GAME.BONUS_FEATURE_STARTED:
                    fireballGCI.gameBonusFeatureStarted(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.BONUS_FEATURE_COMPLETE:
                    fireballGCI.gameBonusFeatureComplete(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.OPEN_URL:
                    fireballGCI.gameOpenUrl(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.SPLASH_SCREEN_VISIBLE:
                    fireballGCI.gameSplashScreenVisible(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.CONFIRM_VISIBLE:
                    fireballGCI.gameConfirmVisible(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.BUY_FEATURE_VISIBLE:
                    fireballGCI.gameBuyFeatureVisible(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.OPEN_DEPOSIT_MENU:
                    fireballGCI.gameOpenDepositMenu();
                    break;
                case FIREBALL_EVENTS.FROM_GAME.ERROR_MESSAGE:
                    fireballGCI.gameErrorMessage(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.LOCK_INTERACTION:
                    fireballGCI.gameLockInteraction(eventValue);
                    break;
                case FIREBALL_EVENTS.FROM_GAME.CLOSED:
                    fireballGCI.gameClosed();
                    break;
                case FIREBALL_EVENTS.FROM_GAME.INTEGRATION_ERROR:
                    fireballGCI.gameIntegrationError(eventValue);
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