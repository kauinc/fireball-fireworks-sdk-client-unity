namespace Fireball.Game.Client.Modules
{
    public interface IFireballLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}