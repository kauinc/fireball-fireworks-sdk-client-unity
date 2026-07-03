using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class JackpotDetailsResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "jackpot-details-result";

        public List<JackpotDetail> Jackpots;

        [UnityEngine.Scripting.Preserve]
        public JackpotDetailsResponse()
        {
            Name = RESPONSE_NAME;
        }

        [UnityEngine.Scripting.Preserve]
        public JackpotDetail DefaultDetails() => new JackpotDetail()
        {
            TemplateId = "",
            Value = 0,
        };
    }

    public class JackpotDetail
    {
        public string TemplateId;
        public long Value;
        public bool OperatorControlled;

        [UnityEngine.Scripting.Preserve]
        public JackpotDetail() { }
    }
}