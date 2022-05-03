using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class JackpotDetailsRequest : BaseRequest
    {
        public const string REQUEST_NAME = "jackpot-details";

        public List<string> JackpotTemplateIds;

        public JackpotDetailsRequest(List<string> jackpotTemplateIds, FireballSession session) : base(REQUEST_NAME, session)
        {
            JackpotTemplateIds = jackpotTemplateIds;
        }
    }
}
