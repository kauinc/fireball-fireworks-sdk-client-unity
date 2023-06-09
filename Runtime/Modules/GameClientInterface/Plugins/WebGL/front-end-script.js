function GetFireballEventName(eventName) {
    if (eventName === 'stopAutoplay') return FIREBALL_GCI_EVENT.STOP_AUTOSPINS;
    else if (eventName === 'setAudio') return FIREBALL_GCI_EVENT.AUDIO_TOGGLE;
    else if (eventName === 'updateBalance') return FIREBALL_GCI_EVENT.UPDATE_BALANCE;
    return null;
}

function GetOperatorEventName(fireballEventName) {
    if (fireballEventName === FIREBALL_GCI_EVENT.LOADING_PROGRESS) return 'loadProgress';
    else if (fireballEventName === FIREBALL_GCI_EVENT.LOADING_COMPLETE) return 'loadCompleted';
    else if (fireballEventName === FIREBALL_GCI_EVENT.AUDIO_TOGGLE) return 'setAudio';
    return null;
}

function SendEventFromGame(event) {
    var eventName = event.detail.name;
    var eventValue = event.detail.value;

    console.log("[OPERATOR-GCI] Game Event Recieved: name = " + eventName + ", value = " + eventValue);
    var operatorEventName = GetOperatorEventName(eventName);
    var message = {
        name: operatorEventName,
        value: eventValue,
    };
    console.log("[OPERATOR-GCI] postMessage: name = ", message);
    //window.parent.postMessage(message, '*');
}

function SendEventToGame(event) {
    var eventName = event.data.name;
    var eventValue = event.data.value;

    console.log("[OPERATOR-GCI] Operator Event Recieved: name = " + eventName + ", value = " + eventValue);
    var fireballEventName = GetFireballEventName(eventName);
    if (fireballEventName) {
        firebalGCI.sendEventToGame(fireballEventName, eventValue);
    }
}

// subscribe for messages from operator page
window.addEventListener("message", SendEventToGame);

// subscribe for messages from game
firebalGCI.addEventListenerFromGame(FIREBALL_GCI_EVENT.AUDIO_TOGGLE, SendEventFromGame);
firebalGCI.addEventListenerFromGame(FIREBALL_GCI_EVENT.LOADING_COMPLETE, SendEventFromGame);
firebalGCI.addEventListenerFromGame(FIREBALL_GCI_EVENT.LOADING_PROGRESS, SendEventFromGame);


