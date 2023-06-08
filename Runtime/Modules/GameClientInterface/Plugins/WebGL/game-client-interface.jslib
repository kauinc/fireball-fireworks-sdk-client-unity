var unityGameClientInterface = {

  sendFirebalGCIEvent: function (eventData) {
    var eventJson = UTF8ToString(eventData);

    console.log('[GCI-JSLIB] sendFirebalGCIEvent: eventJson = ', eventJson);
    var unityEvent = JSON.parse(eventJson);

    console.log('[GCI-JSLIB] sendFirebalGCIEvent: unityEvent = ', unityEvent);
    if (firebalGCI) {
      var customEvent = new CustomEvent(unityEvent.name, {
        detail: {
          name: unityEvent.name,
          value: unityEvent.value,
        },
      });
      console.log('[GCI-JSLIB] sendFirebalGCIEvent: customEvent = ', customEvent);
      firebalGCI.dispatchEventFromGame(customEvent);
    }
    else {
      console.error('[GCI-JSLIB] firebalGCI = null');
    }
  },

};

mergeInto(LibraryManager.library, unityGameClientInterface);