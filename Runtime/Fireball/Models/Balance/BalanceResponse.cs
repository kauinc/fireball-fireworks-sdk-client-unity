using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class BalanceResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "balance-updated";

        public long Balance;

        [UnityEngine.Scripting.Preserve]
        public BalanceResponse()
        {
            Name = RESPONSE_NAME;
        }
    }
}
