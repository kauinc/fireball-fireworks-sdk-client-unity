using System;
using Newtonsoft.Json;

namespace KAU.FireballSDK.Models
{
    public class JackpotUpdateMessage
    {
        public const string MESSAGE_NAME = "jackpotUpdate";

        public string name { get; set; }
        public string templateId { get; set; }
        public long amount { get; set; }
        public long amountPlayerCurrency { get; set; }

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}