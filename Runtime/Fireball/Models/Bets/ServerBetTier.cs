namespace Fireball.Game.Client.Models
{
    public class ServerBetTier
    {
        public string Id { get; set; }
        public string CurrencyIsoCode { get; set; }
        public long Value { get; set; }
        public ServerBetTierDefault BetTier { get; set; }

        [UnityEngine.Scripting.Preserve]
        public ServerBetTier() { }

        public BetTier ToBetTier()
        {
            if (BetTier == null)
            {
                return new BetTier()
                {
                    Tier = 0,
                    Id = this.Id,
                    Value = this.Value,
                    ValueDefault = this.Value,
                    Currency = this.CurrencyIsoCode,
                    CurrencyDefault = this.CurrencyIsoCode,
                };
            }

            return new BetTier()
            {
                Tier = 0,
                Id = this.BetTier.Id,
                Value = this.Value,
                ValueDefault = this.BetTier.Value,
                Currency = this.CurrencyIsoCode,
                CurrencyDefault = this.BetTier.CurrencyIsoCode,
            };
        }
    }
}