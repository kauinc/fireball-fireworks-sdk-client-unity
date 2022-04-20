using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KAU.FireballSDK.Models
{
    public class JackpotDetailsRequest : BaseRequest
    {
        public const string REQUEST_NAME = "jackpot-details";

        public List<string> JackpotTemplateIds { get; set; }
        public string Currency { get; set; }

        public JackpotDetailsRequest(List<string> jackpotTemplateIds, FireballSession session) : base(REQUEST_NAME, session)
        {
            JackpotTemplateIds = jackpotTemplateIds;
            Currency = session.Currency;
        }
    }
}
