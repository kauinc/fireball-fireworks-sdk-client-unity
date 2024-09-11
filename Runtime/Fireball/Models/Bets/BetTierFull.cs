namespace Fireball.Game.Client.Models.Internal
{
    public class BetTierFull
    {
        public string Id { get; set; }
        public string CurrencyIsoCode { get; set; }
        public long Value { get; set; }
        public BetTierDefault BetTier { get; set; }

        [UnityEngine.Scripting.Preserve]
        public BetTierFull() { }

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