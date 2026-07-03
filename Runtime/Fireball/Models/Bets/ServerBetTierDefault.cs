namespace Fireball.Game.Client.Models
{
    public class ServerBetTierDefault
    {
        public string Id { get; set; }
        public long Value { get; set; }
        public string CurrencyIsoCode { get; set; }

        [UnityEngine.Scripting.Preserve]
        public ServerBetTierDefault() { }
    }
}