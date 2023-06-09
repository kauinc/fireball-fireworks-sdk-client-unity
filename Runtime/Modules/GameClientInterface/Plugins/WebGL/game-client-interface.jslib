var unityGameClientInterface = {

  sendFirebalGCIEvent: function (eventData) {
    var eventJson = UTF8ToString(eventData);

    //console.log('[FIREBALL] sendFirebalGCIEvent: eventJson = ', eventJson);
    var unityEvent = JSON.parse(eventJson);

    //console.log('[FIREBALL] sendFirebalGCIEvent: unityEvent = ', unityEvent);
    if (firebalGCI) {
      var customEvent = new CustomEvent(unityEvent.name, {
        detail: {
          name: unityEvent.name,
          value: unityEvent.value,
        },
      });
      //console.log('[FIREBALL] sendFirebalGCIEvent: customEvent = ', customEvent);
      firebalGCI.dispatchEventFromGame(customEvent);
    }
    else {
      console.error('[FIREBALL] firebalGCI = null');
    }
  },

};

mergeInto(LibraryManager.library, unityGameClientInterface);