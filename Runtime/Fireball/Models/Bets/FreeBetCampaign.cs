using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    [UnityEngine.Scripting.Preserve]
    [JsonObject(MemberSerialization.OptIn)]
    public class FreeBetCampaign
    {
        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty]
        public long BetAmount { get; set; }

        [JsonProperty]
        public int NumberOfBets { get; set; }

        [JsonProperty]
        public Dictionary<string, object> Settings { get; set; }
    }
}
