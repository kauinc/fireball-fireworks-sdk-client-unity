namespace Fireball.Game.Client.Models.Internal
{
    public class BetTierDefault
    {
        public string Id { get; set; }
        public long Value { get; set; }
        public string CurrencyIsoCode { get; set; }

        [UnityEngine.Scripting.Preserve]
        public BetTierDefault() { }
    }
}