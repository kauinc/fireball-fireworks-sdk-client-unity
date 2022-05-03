using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class FireballLogger : IFireballLogger
    {
        public void Log(string message) =>
            Debug.Log($"[Fireball] {message}");

        public void LogWarning(string message) =>
            Debug.LogWarning($"[Fireball] {message}");

        public void LogError(string message) =>
            Debug.LogError($"[Fireball] {message}");
    }
}