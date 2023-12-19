using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class FireballLogger : IFireballLogger
    {
        private string _module = string.Empty;
        private LogLevels _level;

        public FireballLogger(LogLevels level = LogLevels.Information)
        {
            _module = string.Empty;
            _level = level;
        }

        public FireballLogger(string module, LogLevels level = LogLevels.Information)
        {
            if (!string.IsNullOrEmpty(module)) _module = $" {module}:";
            _level = level;
        }

        public void Log(string message)
        {
            if (_level <= LogLevels.Debug) Debug.Log($"[Fireball]{_module} {message}");
        }

        public void Info(string message)
        {
            if (_level <= LogLevels.Information) Debug.Log($"[Fireball]{_module} {message}");
        }

        public void Warning(string message)
        {
            if (_level <= LogLevels.Warning) Debug.LogWarning($"[Fireball]{_module} {message}");
        }

        public void Error(string message)
        {
            if (_level <= LogLevels.Error) Debug.LogError($"[Fireball]{_module} {message}");
        }
    }
}