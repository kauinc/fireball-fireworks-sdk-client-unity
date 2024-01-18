using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class FreeBetCampaign
    {
        public string Id { get; set; }
        public long BetAmount { get; set; }
        public int NumberOfBets { get; set; }
        public Dictionary<string, object> Settings { get; set; }

        [UnityEngine.Scripting.Preserve]
        public FreeBetCampaign() { }
    }
}
