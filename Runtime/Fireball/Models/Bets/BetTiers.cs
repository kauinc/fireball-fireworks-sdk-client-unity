using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    internal class BetTiersData
    {
        public List<BetTierFull> Data { get; set; }

        [UnityEngine.Scripting.Preserve]
        public BetTiersData() { }
    }

    internal class BetTierFull
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

    internal class BetTierDefault
    {
        public string Id { get; set; }
        public long Value { get; set; }
        public string CurrencyIsoCode { get; set; }

        [UnityEngine.Scripting.Preserve]
        public BetTierDefault() { }
    }

    public class BetTier
    {
        public int Tier { get; set; }
        public string Id { get; set; }
        public long Value { get; set; }
        public long ValueDefault { get; set; }
        public string Currency { get; set; }
        public string CurrencyDefault { get; set; }

        [UnityEngine.Scripting.Preserve]
        public BetTier() { }
    }
}