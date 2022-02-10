using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Networking;
using KAU.FireballSDK.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KAU.FireballSDK.Tools
{
    public static class URLData
    {
        private const string PARAM_NAME_OPERATOR_ID = "operatorId";
        private const string PARAM_NAME_GAME_ID = "gameId";
        private const string PARAM_NAME_PLAYER_ID = "playerId";
        private const string PARAM_NAME_ENVIRONMENT = "environment";
        private const string PARAM_NAME_LANGUAGE = "language";
        private const string PARAM_NAME_CURRENCY = "currency";
        private const string PARAM_NAME_COUNTRY = "country";
        private const string PARAM_NAME_GENDER = "gender";
        private const string PARAM_NAME_TOKEN = "token";
        private const string PARAM_NAME_MODE = "mode";
        private const string PARAM_NAME_WS_SERVER = "messages";
        private const string PARAM_NAME_WS_TOKEN = "wsToken";
        private const string PARAM_NAME_ROUTER_URL = "router";
        private const string PARAM_NAME_HOME_URL = "home";
        private const string PARAM_NAME_EXTRA = "extra";

        public static FireballSession ParseSessionFromURL()
        {

            Debug.Log($"Get parameters from URL");
            FireballSession session = new FireballSession();
            Dictionary<string, string> extraDict = new Dictionary<string, string>();
            Dictionary<string, string> paramsDict = new Dictionary<string, string>();

/*#if UNITY_EDITOR
            paramsDict = WebLocation.ParseURLParams(GetTestURLParams());
            Debug.Log(GetTestURLParams());
#else
            paramsDict = WebLocation.GetSearchParameters();
#endif*/

            paramsDict = WebLocation.ParseURLParams(GetCleoTestURLParams());
            Debug.Log(GetCleoTestURLParams());

            foreach (string param in paramsDict.Keys)
            {
                if (param.Equals(PARAM_NAME_OPERATOR_ID)) session.OperatorId = paramsDict[param];
                else if (param.Equals(PARAM_NAME_GAME_ID)) session.GameId = paramsDict[param];
                else if (param.Equals(PARAM_NAME_PLAYER_ID)) session.PlayerId = paramsDict[param];
                else if (param.Equals(PARAM_NAME_ENVIRONMENT)) session.Environment = paramsDict[param];
                else if (param.Equals(PARAM_NAME_LANGUAGE)) session.Language = paramsDict[param];
                else if (param.Equals(PARAM_NAME_CURRENCY)) session.Currency = paramsDict[param];
                else if (param.Equals(PARAM_NAME_COUNTRY)) session.Country = paramsDict[param];
                else if (param.Equals(PARAM_NAME_GENDER)) session.Gender = paramsDict[param];
                else if (param.Equals(PARAM_NAME_TOKEN)) session.Token = paramsDict[param];
                else if (param.Equals(PARAM_NAME_WS_SERVER))
                    session.WsServer = UnityWebRequest.UnEscapeURL(paramsDict[param]);
                else if (param.Equals(PARAM_NAME_WS_TOKEN)) session.WsToken = paramsDict[param];
                else if (param.Equals(PARAM_NAME_MODE)) session.Mode = paramsDict[param];
                else if (param.Equals(PARAM_NAME_ROUTER_URL))
                    session.Router = UnityWebRequest.UnEscapeURL(paramsDict[param]);
                else if (param.Equals(PARAM_NAME_HOME_URL))
                    session.HomeUrl = UnityWebRequest.UnEscapeURL(paramsDict[param]);
                else
                {
                    extraDict.Add(param, paramsDict[param]);
                }
            }

            session = ValidateData(session);

            // duplicate 'Currency' param into 'Extra'
            extraDict.Add(PARAM_NAME_CURRENCY, session.Currency);
            //session.Extra = ExtraToString(extraDict);
            session.Extra = extraDict;

            return session;
        }

        private static FireballSession ValidateData(FireballSession session)
        {
            IFireballLogger _logger = new FireballLogger();
            
            // Check URL params
            if (string.IsNullOrEmpty(session.OperatorId))
            {
                _logger.LogWarning("Url params don't contain - operatorId!");
            }
            
            if (string.IsNullOrEmpty(session.GameId))
            {
                _logger.LogWarning("Url params don't contain - gameId!");
            }
            
            if (string.IsNullOrEmpty(session.PlayerId))
            {
                _logger.LogWarning("Url params don't contain - playerId!");
            }
            
            if (string.IsNullOrEmpty(session.Token))
            {
                _logger.LogWarning("Url params don't contain - token!");
            }
            
            if (string.IsNullOrEmpty(session.WsServer))
            {
                _logger.LogWarning("Url params don't contain - messages!");
            }
            
            if (string.IsNullOrEmpty(session.WsToken))
            {
                _logger.LogWarning("Url params don't contain - wsToken!");
            }
            
            if (string.IsNullOrEmpty(session.Language))
            {
                _logger.LogWarning("Url params don't contain - language!");
                session.Language = FireballConfig.DEFAULT_LANGUAGE_CODE;
            }

            if (string.IsNullOrEmpty(session.Currency))
            {
                _logger.LogWarning("Url params don't contain - currency!");
                session.Currency = FireballConfig.DEFAULT_CURRENCY;
            }

            if (string.IsNullOrEmpty(session.Country))
            {
                _logger.LogWarning("Url params don't contain - country!");
            }
            
            if (string.IsNullOrEmpty(session.Gender))
            {
                _logger.LogWarning("Url params don't contain - gender!");
            }
            
            if (string.IsNullOrEmpty(session.Environment))
            {
                _logger.LogWarning("Url params don't contain - Environments!");
                session.Environment = FireballConfig.DEFAULT_ENVIRONMENT.ToString();
            }

            if (string.IsNullOrEmpty(session.Mode))
            {
                _logger.LogWarning("Url params don't contain - mode!");
                session.Mode = FireballConfig.DEFAULT_GAME_MODE.ToString();
            }
            else if (Enum.TryParse(session.Mode, true, out GameMode mode))
            {
                session.Mode = mode.ToString();
            }
            else
            {
                _logger.LogWarning($"Can't parse - mode! Mode String = {session.Mode}");
                session.Mode = FireballConfig.DEFAULT_GAME_MODE.ToString();
            }

            if (Enum.TryParse(session.Environment, true, out Environments environments))
            {
                session.Environment = environments.ToString();
            }
            else
            {
                _logger.LogWarning($"Can't parse - Environments! Environments String = {session.Environment}");
                session.Environment = FireballConfig.DEFAULT_ENVIRONMENT.ToString();
            }

            return session;
        }

//#if UNITY_EDITOR
        private  static string GetTestURLParams()
        {
            GameMode mode = GameMode.money;
            Environments environment = Environments.development;
            string operatorId = "72c43a89-585e-4cf9-8fab-dcc1740eec40";
            string gameId = "b5b6562c-f4a6-4838-86d7-a7bede90ed6c";
            string playerId = "3696f882-7a05-40c7-be2c-49bcf6168fcc";
            string token = "be258078-0d2d-4b49-9b5d-97a087a4c8fd";
            string wsToken = "3112000f-5283-41fa-a692-b8a17784c62c";
            
            return $"?gameId={gameId}" +
                $"&operatorId={operatorId}" +
                $"&playerId={playerId}" +
                $"&environment={environment.ToString()}" +
                $"&token={token}" +
                $"&mode={mode.ToString().ToLower()}" +
                "&router=https%3A%2F%2Fcloud.fireballserver.com%2Frouter" +
                "&messages=wss%3A%2F%2Fcloud.fireballserver.com%2Fmessages" + 
                $"&wsToken={wsToken}" +
                "&platform=web" +
                "&age=36" +
                "&gender=male" +
                "&currency=DKK" +
                "&language=en" +
                "&country=DK" +
                "&siteId=1" +
                "&home=&router=https%3A%2F%2Fcloud.fireballserver.com%2Frouter" +
                "";
        }
        //#endif

        private static string GetCleoTestURLParams()
        {
            // "https://messages-net-dot-fireball-231121.ey.r.appspot.com/messages"

            GameMode mode = GameMode.money;
            Environments environment = Environments.development;
            string operatorId = "72c43a89-585e-4cf9-8fab-dcc1740eec40";
            string gameId = "c61b4556-5816-44bc-845b-39083f19956b";
            string playerId = "566dd8dd-836c-40c1-afeb-dc930ddfac4f";
            string token = "aa2108de-9cfc-484d-a58f-a77e6b4989d6";
            string wsToken = "c55a072f-b941-4ce5-8866-824f147e307e";

            return $"?gameId={gameId}" +
                $"&operatorId={operatorId}" +
                $"&playerId={playerId}" +
                $"&environment={environment.ToString()}" +
                $"&token={token}" +
                $"&mode={mode.ToString().ToLower()}" +
                "&router=https%3A%2F%2Fcloud.fireballserver.com%2Frouter" +
                //"&messages=wss%3A%2F%2Fcloud.fireballserver.com%2Fmessages" +
                //"&messages=https%3A%2F%2Fmessages-net-dot-fireball-231121.ey.r.appspot.com%2Fmessages" +
                "&messages=https%3A%2F%2Fcloud.fireballserver.com%2Fmessages-net%2Fmessages" +
                $"&wsToken={wsToken}" +
                "&platform=web" +
                "&age=36" +
                "&gender=male" +
                "&currency=DKK" +
                "&language=en" +
                "&country=DK" +
                "&siteId=1" +
                "&home=&router=https%3A%2F%2Fcloud.fireballserver.com%2Frouter" +
                "";
        }

    }
}                            