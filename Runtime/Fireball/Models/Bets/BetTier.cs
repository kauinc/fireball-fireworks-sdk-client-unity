using System;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
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