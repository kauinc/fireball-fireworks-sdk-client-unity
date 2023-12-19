using System;
using System.Collections.Generic;
using Fireball.Game.Client.Tools;
using Newtonsoft.Json;
using UnityEngine;

namespace Fireball.Game.Client
{
    [Serializable]
    [CreateAssetMenu(fileName ="FireballSettings", menuName = "Fireball/New Settings")]
    public class FireballSettings : ScriptableObject
    {
        [Header("Game Data (Required)")]
        public GameMode GameMode = FireballConfig.DEFAULT_GAME_MODE;
        public Environments Environment = FireballConfig.DEFAULT_ENVIRONMENT;
        public string OperatorId = string.Empty;
        public string GameId = string.Empty;

        [Header("Player Data (Required)")]
        public string OperatorPlayerId = string.Empty;
        public string Token = string.Empty;

        [Header("Player Info (Optional)")]
        public string Language = FireballConfig.DEFAULT_LANGUAGE_CODE;
        public string Currency = FireballConfig.DEFAULT_CURRENCY;
        public string Country = string.Empty;
        public string Gender = string.Empty;

        public List<SimpleKeyValue<string>> Extra = new List<SimpleKeyValue<string>>();

        public FireballSession GetSession()
        {
            var extra = this.Extra.ToDictionary();
            extra.Add("operatorPlayerId", OperatorPlayerId);

            return new FireballSession()
            {
                GameMode = this.GameMode.ToString(),
                Environment = this.Environment.ToString(),
                OperatorId = this.OperatorId,
                GameId = this.GameId,

                OperatorPlayerId = this.OperatorPlayerId,
                Token = this.Token,

                Language = this.Language,
                Currency = this.Currency,
                Country = this.Country,
                Gender = this.Gender,

                Router = FireballConfig.URL_ROUTER_DEFAULT,
                WsServer = FireballConfig.URL_MESENGER_DEFAULT,

                Extra = extra,
            };
        }

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
}
