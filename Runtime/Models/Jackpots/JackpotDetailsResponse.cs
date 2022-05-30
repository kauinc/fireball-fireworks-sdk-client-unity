using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class JackpotDetailsResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "jackpot-details-result";

        public List<JackpotDetail> Jackpots;

        public JackpotDetailsResponse()
        {
            Name = RESPONSE_NAME;
        }
    }

    public class JackpotDetail
    {
        public string TemplateId;
        public string Value;
        public bool OperatorControlled;
    }
}