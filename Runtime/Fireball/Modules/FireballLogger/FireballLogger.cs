using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class FireballLogger : IFireballLogger
    {
        private string _module = string.Empty;
        public static LogLevels LogLevel = LogLevels.Information;

        public FireballLogger()
        {
            _module = string.Empty;
        }

        public FireballLogger(string module)
        {
            if (!string.IsNullOrEmpty(module)) _module = $" {module}:";
        }

        public void Log(string message)
        {
            if (LogLevel <= LogLevels.Debug) Debug.Log($"[Fireball]{_module} {message}");
        }

        public void Info(string message)
        {
            if (LogLevel <= LogLevels.Information) Debug.Log($"[Fireball]{_module} {message}");
        }

        public void Warning(string message)
        {
            if (LogLevel <= LogLevels.Warning) Debug.LogWarning($"[Fireball]{_module} {message}");
        }

        public void Error(string message)
        {
            if (LogLevel <= LogLevels.Error) Debug.LogError($"[Fireball]{_module} {message}  ({LogLevel})");
        }
    }
}