namespace KAU.FireballSDK.Modules
{
    public interface IFireballLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}