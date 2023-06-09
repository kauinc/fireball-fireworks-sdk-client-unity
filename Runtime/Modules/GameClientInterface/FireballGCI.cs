using System;
using UnityEngine;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace Fireball.Game.Client.Modules
{
    public class FireballGCI
    {
        private static FireballGCIEventsReceiver _eventsListener = null;
        private IFireballLogger _logger = null;

        public static string EVENT_STOP_AUTOSPINS = "StopAutospins";
        public static string EVENT_AUDIO_TOGGLE = "AudioToggle";
        public static string EVENT_UPDATE_BALANCE = "UpdateBalance";
        public static string EVENT_LOADING_COMPLETE = "LoadingComplete";
        public static string EVENT_LOADING_PROGRESS = "LoadingProgress";

        public Action OnStopAutospins;
        public Action<float> OnAudioToggle;
        public Action<string> OnBalanceUpdated;

        public FireballGCI(IFireballLogger logger)
        {
            _logger = logger;
            if (_eventsListener == null)
            {
                var go = new GameObject(nameof(FireballGCIEventsReceiver));
                _eventsListener = go.AddComponent<FireballGCIEventsReceiver>();
                _eventsListener.Init(this);
            }
        }

        public void SendLoadingProgress(float percent)
        {
            _logger.Info($"GCI SendEvent LoadingProgress: {percent}");
            var eventData = new FireballGCIEvent(EVENT_LOADING_PROGRESS, percent);
            sendFirebalGCIEvent(eventData.ToJson());
        }

        public void SendLoadingComplete()
        {
            _logger.Info($"GCI SendEvent LoadingComplete");
            var eventData = new FireballGCIEvent(EVENT_LOADING_COMPLETE);
            sendFirebalGCIEvent(eventData.ToJson());
        }

        public void SendAudioToggle(float percent)
        {
            _logger.Info($"GCI SendEvent AudioToggle: {percent}");
            var eventData = new FireballGCIEvent(EVENT_AUDIO_TOGGLE, percent);
            sendFirebalGCIEvent(eventData.ToJson());
        }

#if UNITY_EDITOR
        private static void sendFirebalGCIEvent(string eventJson) => Debug.LogWarning($"[GCI-UNITY] Not working in Unity Editor");
#elif UNITY_WEBGL
        [DllImport("__Internal")] private static extern void sendFirebalGCIEvent(string eventJson);
#else
        private static void sendFirebalGCIEvent(string eventJson) => Debug.LogWarning($"[GCI-UNITY] Not implemented for current platform");
#endif

    }
}
