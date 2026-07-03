namespace Fireball.Game.Client.Modules
{
    public interface IFireballLogger
    {
        void Log(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
}