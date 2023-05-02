using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class BetTiers
    {
        public string Currency { get; set; }
        public List<TierData> Tiers { get; set; }
    }

    public class TierData
    {
        public int Tier { get; set; }
        public long Value { get; set; }
    }
}