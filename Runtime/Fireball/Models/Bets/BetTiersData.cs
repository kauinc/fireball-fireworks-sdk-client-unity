using System.Collections.Generic;
using Fireball.Game.Client.Models.Internal;

namespace Fireball.Game.Client.Models
{
    public class BetTiersData
    {
        public List<BetTierFull> Data { get; set; }

        [UnityEngine.Scripting.Preserve]
        public BetTiersData() { }
    }
}
