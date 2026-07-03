using System.Collections.Generic;

namespace Fireball.Game.Client.Models
{
    public class ServerBetTiersData
    {
        public List<ServerBetTier> Data { get; set; }

        [UnityEngine.Scripting.Preserve]
        public ServerBetTiersData() { }
    }
}
