using System;
using UnityEngine;

#if UNITY_WEBGL
using AOT;
using System.Runtime.InteropServices;
#endif

namespace Fireball.Game.Client.Modules
{
    public enum VisibilityOption
    {
        Hide = 0,
        Show = 1,
        Toggle = 2,
    }

    public class FireballGCI
    {
        private static FireballGCI _instance = null;

        public delegate void EventJsonDelegate(System.IntPtr ptr);
        private static event Action<string> OnReceivedEventJson;

        private static ModuleLogger _logger = null;

        private FireballGCI()
        {
            if (_logger == null)
            {
                _logger = new ModuleLogger("GCI");
            }
        }

        public static FireballGCI GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FireballGCI();
                init(onEventRecieved);
                OnReceivedEventJson += _instance.ParseReceivedEventJson;
            }

            return _instance;
        }

        #region OPERATORS_EVENTS

        /// <summary>
        /// Operator's page ask game to set new volume value. 0 - means mute, from 0 to 1 - current volume level
        /// </summary>
        public Action<float> OnAudioVolume;

        /// <summary>
        /// Operator's page ask game enable/disable bet turbo (turbo spin/ fast play) feature.
        /// <code>NOTE:</code> Used only if game support this feature
        /// </summary>
        public Action<bool> OnBetTurbo; // new

        /// <summary>
        /// Operator's page ask game start spin/round/buy a game with specific bet value. Bet value sends in cents.
        /// <code>NOTE:</code> Required by some operators
        /// </summary>
        public Action<long> OnBetPlace; // new

        /// <summary>
        /// Operator's page ask game to update current bet value. New bet value sends in cents.
        /// <code>NOTE:</code> Required by some operators
        /// </summary>
        public Action<long> OnBetUpdate; // new

        /// <summary>
        /// Operator's page send request about balance have been changed outside the game (ex: after player makes deposit).
        /// Game need to ask remote server for new updated balance and after notify operator with game event:
        /// <code>SendBalanceUpdated(balance)</code>
        /// </summary>
        public Action OnBalanceUpdated;

        /// <summary>
        /// Operator's page ask game to stop autoplay.
        /// </summary>
        public Action OnStopAutoplay;

        /// <summary>
        /// Operator's page ask game to show/hide/toggle in game Help screen.
        /// </summary>
        public Action<VisibilityOption> OnVisibleHelp;

        /// <summary>
        /// Operator's page ask game to show/hide/toggle in game Paytable screen.
        /// </summary>
        public Action<VisibilityOption> OnVisiblePaytable;

        /// <summary>
        /// Operator's page ask game to confirm and close previosly opened popup/screen. Used with event sent by game client:
        /// <code>SendConfirmVisible(true)</code>
        /// Game must confirm action on popup/screen, hide it and invoke method
        /// <code>SendConfirmVisible(false)</code>
        /// </summary>
        public Action OnConfirmedAction; // new

        /// <summary>
        /// Operator's page ask to pause/resume the game. Pausing means prevent further gameplay, and block user interaction.
        /// </summary>
        public Action<bool> OnPauseGame; // new

        /// <summary>
        /// Operator's page ask to close the game. Go to home page method can be used.
        /// </summary>
        public Action OnCloseGame; // new

        /// <summary>
        /// Used for parse and invoke events received from operator's page
        /// </summary>
        /// <param name="eventJson"></param>
        public void ParseReceivedEventJson(string eventJson)
        {
            try
            {
                _logger.Log($"ReceivedEvent: {eventJson}");
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
                        OnBalanceUpdated?.Invoke();
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_STOP_AUTOPLAY:
                        OnStopAutoplay?.Invoke();
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_VISIBLE_HELP:
                        VisibilityOption visibleHelp = eventData.value != null ? (VisibilityOption)(long)eventData.value : VisibilityOption.Toggle;
                        OnVisibleHelp?.Invoke(visibleHelp);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_VISIBLE_PAYTABLE:
                        VisibilityOption visiblePaytable = eventData.value != null ? (VisibilityOption)(long)eventData.value : VisibilityOption.Toggle;
                        OnVisiblePaytable?.Invoke(visiblePaytable);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_CONFIRMED_ACTION:
                        OnConfirmedAction?.Invoke();
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_PAUSE_GAME:
                        bool pause = eventData.value != null ? (bool)eventData.value : false;
                        OnPauseGame?.Invoke(pause);
                        break;
                    case FireballGCIEvent.EVENT_OPERATOR_CLOSE_GAME:
                        OnCloseGame?.Invoke();
                        break;
                    default:
                        _logger.Warning($"ReceivedEvent: undefined event with name - {eventData?.name}");
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Events Receiver Exception: {e}");
            }
        }

        #endregion OPERATORS_EVENTS

        #region GAME_EVENTS

        /// <summary>
        /// Sends when game loading process start
        /// </summary>
        public void SendLoadingStarted() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_LOADING_STARTED);
        }

        /// <summary>
        /// Sends when game loading progress changed
        /// </summary>
        /// <param name="percent">loading progress</param>
        public void SendLoadingProgress(float percent)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_LOADING_PROGRESS, percent);
        }

        /// <summary>
        /// Sends when game loading progress completed
        /// </summary>
        public void SendLoadingComplete()
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_LOADING_COMPLETE);
        }

        /// <summary>
        /// Sends when game ready to play - all intro videos, splash screens, promo complete and player can start spin/new round/buy game
        /// </summary>
        public void SendReadyToPlay() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_READY_PLAY);
        }

        /// <summary>
        /// Sends when game audio volume has changed. 0 - means mute, from 0 to 1 - current volume level
        /// </summary>
        /// <param name="percent">volume level</param>
        public void SendAudioVolume(float percent)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_AUDIO_VOLUME, percent);
        }

        /// <summary>
        /// Sends when bet turbo (turbo spin, fast play) feature enable/disable in game
        /// <code>NOTE:</code> Used only if game support this feature
        /// </summary>
        /// <param name="enabled"></param>
        public void SendBetTurbo(bool enabled) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BET_TURBO, enabled);
        }

        /// <summary>
        /// Send when bet placed - start spin, new round or buy a game - with specific bet value. Bet value sends in cents.
        /// </summary>
        /// <param name="betValue"></param>
        public void SendBetPlaced(long betValue)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BET_PLACED, betValue);
        }

        /// <summary>
        /// Sends when bet result received - spin result, round ended, game paid - with specific win amount.
        /// Win value sends in cents.
        /// For lose result send zero win value.
        /// </summary>
        /// <param name="winValue"></param>
        public void SendBetResult(long winValue)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BET_RESULT, winValue);
        }

        /// <summary>
        /// Sends when bet value changed in game. Bet value sends in cents.
        /// </summary>
        /// <param name="betValue"></param>
        public void SendBetUpdate(long betValue)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BET_UPDATE, betValue);
        }

        /// <summary>
        /// Sends when in-game balance have been updated after all animations and counting complete if they are present in game.
        /// Used to sync in-game balance with operator's page balance.
        /// Balance sends in cents.
        /// </summary>
        /// <param name="balance"></param>
        public void SendBalanceUpdated(long balance) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BALANCE_UPDATED, balance);
        }

        /// <summary>
        /// Sends when in-game jackpot have been updated after all animations, conting complete.
        /// Used to sync in-game jackpot value with operator's page balance.
        /// Jackpot value sends in cents.
        /// <code>NOTE:</code> Used only if game support this feature
        /// </summary>
        /// <param name="jackpotTemlateId"></param>
        /// <param name="jackpotValue"></param>
        public void SendJackpotUpdated(string jackpotTemlateId, long jackpotValue) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_JACKPOT_UPDATED, new JackpotData(jackpotTemlateId, jackpotValue));
        }

        /// <summary>
        /// Sends when autoplay feature enabled
        /// <code>NOTE:</code> Used only if game support this feature
        /// </summary>
        public void SendAutoplayStarted() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_AUTOPLAY_STARTED);
        }

        /// <summary>
        /// Sends when autoplay feature stoped or ended
        /// <code>NOTE:</code> Used only if game support this feature
        /// </summary>
        public void SendAutoplayComplete() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_AUTOPLAY_COMPLETE);
        }

        /// <summary>
        /// Sends when some in-game bonus feature started
        /// <code>NOTE:</code> Used only if game support this feature
        /// </summary>
        public void SendBonusFeatureStarted(string bonusFeaturetype) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BONUS_FEATURE_STARTED, bonusFeaturetype);
        }

        /// <summary>
        /// Sends when some in-game bonus feature completed
        /// <code>NOTE:</code> Used only if game support this feature
        /// </summary>
        public void SendBonusFeatureComplete(string bonusFeaturetype) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BONUS_FEATURE_COMPLETE, bonusFeaturetype);
        }

        /// <summary>
        /// Sends when game needs that operator's page navigate to some url target.
        /// <code>NOTE:</code> supported only by some operators
        /// </summary>
        public void SendOpenUrl(string url) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_OPEN_URL, url);
        }

        /// <summary>
        /// Sends when game startup splash screen containing game description/mechanics or promo screen is displayed or closed
        /// <code>NOTE:</code> Used only if game has this feature
        /// </summary>
        /// <param name="visible"></param>
        public void SendSplashScreenVisible(bool visible) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_SPLASH_SCREEN_VISIBLE, visible);
        }

        /// <summary>
        /// Sends when any in-game screen/popup displayed or closed that need players confirmation.
        /// Can be used together with operator's <c>OnConfirmedAction</c> action that means confirm with opened game screen/popup and close it
        /// </summary>
        /// <param name="visible"></param>
        public void SendConfirmVisible(bool visible) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_CONFIRM_VISIBLE, visible);
        }

        /// <summary>
        /// Sends when some game menu opened/hided where player can directly buy some game bonus or feature
        /// <code>NOTE:</code> Used only if game support this feature
        /// </summary>
        /// <param name="visible"></param>
        public void SendBuyFeatureVisible(bool visible) // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_BUY_FEATURE_VISIBLE, visible);
        }

        /// <summary>
        /// Sends from game to operator's page for open deposit menu/page.
        /// <code>NOTE:</code> supported only by some operators
        /// </summary>
        public void SendOpenDepositMenu() // new 
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_OPEN_DEPOSIT_MENU);
        }

        /// <summary>
        /// Sends when any in-game error occured
        /// </summary>
        /// <param name="message"></param>
        public void SendErrorMessage(string message)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_ERROR_MESSAGE, message);
        }

        /// <summary>
        /// Sends during long in-game win animations or game feature mechanics that blocks player interaction with game
        /// <code>NOTE:</code> supported only by some operators
        /// </summary>
        /// <param name="locked"></param>
        public void SendLockInteraction(bool locked)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_LOCK_INTERACTION, locked);
        }

        /// <summary>
        /// Sends when game fully closed
        /// </summary>
        public void SendGameClosed()
        {
            SendGCIEvent(FireballGCIEvent.EVENT_GAME_CLOSED);
        }

        /// <summary>
        /// Automatically sends by fireball when get message or error from integration
        /// </summary>
        /// <param name="message"></param>
        public void SendIntegrationError(object message)
        {
            SendGCIEvent(FireballGCIEvent.EVENT_INTEGRATION_ERROR, message);
        }

        /// <summary>
        /// Send any custom event to operator's page
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventValue"></param>
        private void SendGCIEvent(string eventName, object eventValue = null)
        {
            _logger.Log($"SendEvent: {eventName} - {eventValue}");
            var eventData = new FireballGCIEvent(eventName, eventValue);
            sendFireballGCIEvent(eventData.ToJson());
        }

        #endregion GAME_EVENTS

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern bool isInit();
        [DllImport("__Internal")] private static extern void init(EventJsonDelegate eventCallback);
        [DllImport("__Internal")] private static extern void sendFireballGCIEvent(string eventJson);
        [MonoPInvokeCallback(typeof(EventJsonDelegate))]
        private static void onEventRecieved(System.IntPtr ptr)
        {
            OnReceivedEventJson?.Invoke(Marshal.PtrToStringAuto(ptr));
        }
#elif UNITY_EDITOR
        private static bool isInit() => true;
        private static void init(EventJsonDelegate eventCallback) => _logger.Warning($"Not working in Unity editor");
        private static void onEventRecieved(System.IntPtr ptr) => _logger.Warning($"Not working in Unity editor");
        private static void sendFireballGCIEvent(string eventJson) => _logger.Warning($"Not working in Unity editor");
#else
        private static bool isInit() => true;
        private static void init(EventJsonDelegate eventCallback) => _logger.Warning($"Not implemented for current platform");
        private static void onEventRecieved(System.IntPtr ptr) => _logger.Warning($"Not implemented for current platform");
        private static void sendFireballGCIEvent(string eventJson) => _logger.Warning($"Not implemented for current platform");
#endif

        public class JackpotData
        {
            public string TemplateId;
            public long Value;

            [UnityEngine.Scripting.Preserve]
            public JackpotData() { }

            public JackpotData(string templateId, long value)
            {
                TemplateId = templateId;
                Value = value;
            }

            public override string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }
        }
    }
}
