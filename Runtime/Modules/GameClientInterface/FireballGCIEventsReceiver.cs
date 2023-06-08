using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class FireballGCIEventsReceiver : MonoBehaviour
    {
        private FireballGCI _gci;

        public void Init(FireballGCI gci)
        {
            _gci = gci;
            DontDestroyOnLoad(this);
        }

        public void StopAutospins()
        {
            Debug.Log($"[GCI-UNITY] Received StopAutospins");
            _gci.OnStopAutospins?.Invoke();
        }

        public void AudioToggle(float percent)
        {
            Debug.Log($"[GCI-UNITY] Received AudioToggle: {percent}");
            _gci.OnAudioToggle?.Invoke(percent);
        }

        public void UpdateBalance(string balance)
        {
            Debug.Log($"[GCI-UNITY] Received BalanceUpdated: {balance}");
            _gci.OnBalanceUpdated?.Invoke(balance);
        }
    }
}
