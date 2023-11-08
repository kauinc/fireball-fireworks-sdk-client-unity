using System;
using UnityEngine;

#if UNITY_WEBGL
using AOT;
using System.Runtime.InteropServices;
#endif

namespace Fireball.Game.Client.Modules
{
    public class FireballGCI
    {
        private static FireballGCI _instance = null;

        public delegate void EventJsonDelegate(System.IntPtr ptr);
        private static event Action<string> OnReceivedEventJson;

        private IFireballLogger _logger = null;

        public Action<float> OnAudioVolume;
        public Action<bool> OnBetTurbo; // new 
        public Action<long> OnBetPlace; // new 
        public Action<long> OnBetUpdate; // new 
        public Action<long> OnBalanceUpdated;
        public Action OnStopAutoplay;
        public Action<bool> OnVisibleHelp;
        public Action<bool> OnVisiblePaytable;
        public Action<bool> OnPauseGame; // new 

        private FireballGCI(IFireballLogger logger)
        {
            _logger = logger;
        }

        public static FireballGCI GetInstance(IFireballLogger logger)
        {
            if (_instance == null)
            {
                _instance = new FireballGCI(logger);
                init(onEventRecieved);
                OnReceivedEventJson += _instance.ParseReceivedEventJson;
            }

            return _instance;
        }

        public void SendLoadingStarted() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_LOADING_STARTED);
        }
        public void SendLoadingProgress(float percent)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_LOADING_PROGRESS, percent);
        }
        public void SendLoadingComplete()
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_LOADING_COMPLETE);
        }
        public void SendReadyToPlay() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_READY_PLAY);
        }
        public void SendAudioVolume(float percent)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_AUDIO_VOLUME, percent);
        }
        public void SendBetTurbo(bool enabled) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BET_TURBO, enabled);
        }
        public void SendBetPlaced(long betValue)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BET_PLACED, betValue);
        }
        public void SendBetResult(long winValue)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BET_RESULT, winValue);
        }
        public void SendBetUpdate(long betValue)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BET_UPDATE, betValue);
        }
        public void SendBalanceUpdated(long balance) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BALANCE_UPDATED, balance);
        }
        public void SendAutoplayStarted() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_AUTOPLAY_STARTED);
        }
        public void SendAutoplayComplete() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_AUTOPLAY_COMPLETE);
        }
        public void SendBonusFeatureStarted() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BONUS_FEATURE_STARTED);
        }
        public void SendBonusFeatureComplete() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BONUS_FEATURE_COMPLETE);
        }
        public void SendOpenUrl(string url) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_OPEN_URL, url);
        }
        public void SendErrorMessage(string message)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_ERROR_MESSAGE, message);
        }
        public void SendGameClosed()
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_CLOSED);
        }

        public void SendIntegrationError(object message)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_INTEGRATION_ERROR, message);
        }

        private void SendGCIEvent(string eventName, object eventValue = null)
        {
            _logger.Info($"GCI: SendEvent: {eventName} - {eventValue}");
            var eventData = new FireballGCIEvent(eventName, eventValue);
            sendFireballGCIEvent(eventData.ToJson());
        }
        public void ParseReceivedEventJson(string eventJson)
        {
            try
            {
                _logger.Info($"GCI: ReceivedEvent: {eventJson}");
                var eventData = Newtonsoft.Json.JsonConvert.DeserializeObject<FireballGCIEvent>(eventJson);
                switch (eventData.name)
                {
                    case FireballGCIEvent.EVENT_OPERATOR_AUDIO_VOLUME:
                        float volume = eventData.value != null ? float.Parse(eventData.value.ToString()) : 0.0f;
                        OnAudioVolume?.Invoke(volume);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_BET_TURBO:
                        bool enabled = eventData.value != null ? (bool)eventData.value : false;
                        OnBetTurbo?.Invoke(enabled);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_BET_PLACE:
                        long betValue = eventData.value != null ? (long)eventData.value : 0;
                        OnBetPlace?.Invoke(betValue);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_BET_UPDATE:
                        long newBetValue = eventData.value != null ? (long)eventData.value : 0;
                        OnBetUpdate?.Invoke(newBetValue);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_UPDATE_BALANCE:
                        long balance = eventData.value != null ? (long)eventData.value : 0;
                        OnBalanceUpdated?.Invoke(balance);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_STOP_AUTOPLAY:
                        OnStopAutoplay?.Invoke();
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_VISIBLE_HELP:
                        bool visibleHelp = eventData.value != null ? (bool)eventData.value : true;
                        OnVisibleHelp?.Invoke(visibleHelp);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_VISIBLE_PAYTABLE:
                        bool visiblePaytable = eventData.value != null ? (bool)eventData.value : true;
                        OnVisiblePaytable?.Invoke(visiblePaytable);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_PAUSE_GAME:
                        bool pause = eventData.value != null ? (bool)eventData.value : false;
                        OnPauseGame?.Invoke(pause);
                        break;
                    default:
                        _logger.Warning($"GCI: ReceivedEvent: undefined event with name - {eventData?.name}");
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"GCI: Events Receiver Exception: {e}");
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern bool isInit();
        [DllImport("__Internal")] private static extern void init(EventJsonDelegate eventCallback);
        [DllImport("__Internal")] private static extern void sendFireballGCIEvent(string eventJson);
        [MonoPInvokeCallback(typeof(EventJsonDelegate))]
        private static void onEventRecieved(System.IntPtr ptr)
        {
            OnReceivedEventJson?.Invoke(Marshal.PtrToStringAuto(ptr));
        }
#else
        private static bool isInit() => true;
        private static void init(EventJsonDelegate eventCallback) => Debug.LogWarning($"[Fireball] GCI: Not implemented for current platform");
        private static void onEventRecieved(System.IntPtr ptr) => Debug.LogWarning($"[Fireball] GCI: Not implemented for current platform");
        private static void sendFireballGCIEvent(string eventJson) => Debug.LogWarning($"[Fireball] GCI: Not implemented for current platform");
#endif

    }
}
