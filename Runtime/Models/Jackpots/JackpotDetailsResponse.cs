using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KAU.FireballSDK.Models
{
    public class JackpotDetailsResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "jackpot-details-result";

        public List<JackpotDetail> Jackpots { get; set; }

        public JackpotDetailsResponse()
        {
            Name = RESPONSE_NAME;
        }
    }

    public class JackpotDetail
    {
        public string TemplateId { get; set; }
        public string Value { get; set; }
    }
}