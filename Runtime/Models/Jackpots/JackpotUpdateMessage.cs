using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class JackpotUpdateMessage
    {
        public const string MESSAGE_NAME = "jackpotUpdate";

        public string name;
        public string templateId;
        public long amount;
        public long amountPlayerCurrency;

        [UnityEngine.Scripting.Preserve]
        public JackpotUpdateMessage() { }

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}