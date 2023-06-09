using System;
using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class FireballGCIEventsReceiver : MonoBehaviour
    {
        public Action<FireballGCIEvent> OnEventReceived;

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void ReceiveEvent(string eventJson)
        {
            try
            {
                var eventData = Newtonsoft.Json.JsonConvert.DeserializeObject<FireballGCIEvent>(eventJson);
                OnEventReceived?.Invoke(eventData);
            }
            catch(Exception e)
            {
                Debug.LogError($"[FIREBALL] GCI Events Receiver Exception: {e}");
            }
        }
    }
}
