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

        public const string EVENT_OPERATOR_AUDIO_VOLUME = "operator_audio_volume";
        public const string EVENT_OPERATOR_STOP_AUTOPLAY = "operator_stop_autoplay";
        public const string EVENT_OPERATOR_UPDATE_BALANCE = "operator_update_balance";

        public const string EVENT_GAME_AUDIO_VOLUME = "game_audio_volume";
        public const string EVENT_GAME_LOADING_COMPLETE = "game_loading_complete";
        public const string EVENT_GAME_LOADING_PROGRESS = "game_loading_progress";

        public Action OnStopAutoplay;
        public Action<float> OnAudioVolume;
        public Action<long> OnBalanceUpdated;

        public FireballGCI(IFireballLogger logger)
        {
            _logger = logger;
            if (_eventsListener == null)
            {
                var go = new GameObject(nameof(FireballGCIEventsReceiver));
                _eventsListener = go.AddComponent<FireballGCIEventsReceiver>();
                _eventsListener.OnEventReceived += OnReceivedEvent;
            }
        }

        public void SendLoadingProgress(float percent)
        {
            SendGCIEvent(EVENT_GAME_LOADING_PROGRESS, percent);
        }

        public void SendLoadingComplete()
        {
            SendGCIEvent(EVENT_GAME_LOADING_COMPLETE);
        }

        public void SendAudioVolume(float percent)
        {
            SendGCIEvent(EVENT_GAME_AUDIO_VOLUME, percent);
        }

        private void SendGCIEvent(string eventName, object eventValue = null)
        {
            _logger.Info($"GCI SendEvent: {eventName} - {eventValue}");
            var eventData = new FireballGCIEvent(eventName, eventValue);
            sendFirebalGCIEvent(eventData.ToJson());
        }

        private void OnReceivedEvent(FireballGCIEvent eventData)
        {
            _logger.Info($"GCI ReceivedEvent: {eventData.ToJson()}");

            switch (eventData.name)
            {
                case FireballGCI.EVENT_OPERATOR_STOP_AUTOPLAY:
                    OnStopAutoplay?.Invoke();
                    break;

                case FireballGCI.EVENT_OPERATOR_AUDIO_VOLUME:
                    float volume = eventData.value != null ? float.Parse(eventData.value.ToString()) : 0.0f;
                    OnAudioVolume?.Invoke(volume);
                    break;

                case FireballGCI.EVENT_OPERATOR_UPDATE_BALANCE:
                    long balance = eventData.value != null ? (long)eventData.value : 0;
                    OnBalanceUpdated?.Invoke(balance);
                    break;

                default:
                    _logger.Warning($"GCI ReceivedEvent: undefined event with name - {eventData?.name}");
                    break;
            }
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
