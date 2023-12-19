using UnityEngine;

namespace Fireball.Game.Client.Modules
{
    public class ModuleLogger
    {
        public enum LogLevels
        {
            Debug = 0,
            Information = 1,
            Warning = 2,
            Error = 3,
            None = 4,
        }

        private string _module = string.Empty;
        public static int LogLevel = (int)LogLevels.Information;

        public ModuleLogger()
        {
            _module = string.Empty;
        }

        public ModuleLogger(string module)
        {
            if (!string.IsNullOrEmpty(module)) _module = $" {module}:";
        }

        public void Log(string message)
        {
            if (LogLevel <= (int)LogLevels.Debug) Debug.Log($"[Fireball]{_module} {message}");
        }

        public void Info(string message)
        {
            if (LogLevel <= (int)LogLevels.Information) Debug.Log($"[Fireball]{_module} {message}");
        }

        public void Warning(string message)
        {
            if (LogLevel <= (int)LogLevels.Warning) Debug.LogWarning($"[Fireball]{_module} {message}");
        }

        public void Error(string message)
        {
            if (LogLevel <= (int)LogLevels.Error) Debug.LogError($"[Fireball]{_module} {message}");
        }
    }
}
