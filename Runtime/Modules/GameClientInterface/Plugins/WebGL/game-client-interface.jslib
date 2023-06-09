var unityGameClientInterface = {

  sendFirebalGCIEvent: function (eventData) {
    var eventJson = UTF8ToString(eventData);
    var unityEvent = JSON.parse(eventJson);
    if (firebalGCI) {
      var customEvent = new CustomEvent(unityEvent.name, {
        detail: {
          name: unityEvent.name,
          value: unityEvent.value,
        },
      });
      firebalGCI.dispatchEventFromGame(customEvent);
    }
    else {
      console.error('[FIREBALL] firebalGCI = null');
    }
  },

};

mergeInto(LibraryManager.library, unityGameClientInterface);