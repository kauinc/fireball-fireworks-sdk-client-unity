using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Modules
{
    public class Translation
    {
        private IFireballLogger _logger;
        private Communicator _communicator;

        public Translation(Communicator communicator, IFireballLogger logger)
        {
            _communicator = communicator;
            _logger = logger;
        }


        public void GetTranslation<T>(string gameId, string languageCode, Action<TranslationData<T>> onSuccess, Action<string> onError = null)
        {
            var query = new Dictionary<string, string>()
            {
                { "appId", gameId },
                { "languageIsoCode", languageCode }
            };
            _communicator.SendGET(FireballConfig.URL_TRANSLATION, query,
                (json) =>
                {
                    try
                    {
                        _logger.Log($"On Translation: success! {json}");
                        var result = JsonConvert.DeserializeObject<TranslationResponse<T>>(json, new JsonSerializerSettings()
                        {
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                        });
                        onSuccess?.Invoke(result.Data);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"On Translation: Error - can't parse json! Exception: {e.Message}");
                        onError?.Invoke(e.Message);
                    }
                },
                onError);
        }
    }
}
