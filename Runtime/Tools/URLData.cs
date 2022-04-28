using System;
using System.Collections.Generic;
using KAU.FireballSDK.Modules;
using UnityEngine;
using UnityEngine.Networking;

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

        public static FireballSession ParseSessionFromURL(string customUrl = null)
        {
            Debug.Log($"Get parameters from URL");
            FireballSession session = new FireballSession();
            Dictionary<string, string> extraDict = new Dictionary<string, string>();
            Dictionary<string, string> paramsDict = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(customUrl))
            {
                paramsDict = WebLocation.GetSearchParameters();
            }
            else
            {
                paramsDict = WebLocation.ParseURLParams(customUrl);
            }

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
                else if (param.Equals(PARAM_NAME_MODE)) session.GameMode = paramsDict[param];
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
            IFireballLogger logger = new FireballLogger();
            
            // Check URL params
            if (string.IsNullOrEmpty(session.OperatorId))
            {
                logger.LogWarning("Url params don't contain - operatorId!");
            }
            
            if (string.IsNullOrEmpty(session.GameId))
            {
                logger.LogWarning("Url params don't contain - gameId!");
            }
            
            if (string.IsNullOrEmpty(session.PlayerId))
            {
                logger.LogWarning("Url params don't contain - playerId!");
            }
            
            if (string.IsNullOrEmpty(session.Token))
            {
                logger.LogWarning("Url params don't contain - token!");
            }
            
            if (string.IsNullOrEmpty(session.WsServer))
            {
                logger.LogWarning("Url params don't contain - messages!");
            }
            
            if (string.IsNullOrEmpty(session.WsToken))
            {
                logger.LogWarning("Url params don't contain - wsToken!");
            }
            
            if (string.IsNullOrEmpty(session.Language))
            {
                logger.LogWarning("Url params don't contain - language!");
                session.Language = FireballConfig.DEFAULT_LANGUAGE_CODE;
            }

            if (string.IsNullOrEmpty(session.Currency))
            {
                logger.LogWarning("Url params don't contain - currency!");
                session.Currency = FireballConfig.DEFAULT_CURRENCY;
            }

            if (string.IsNullOrEmpty(session.Country))
            {
                logger.LogWarning("Url params don't contain - country!");
            }
            
            if (string.IsNullOrEmpty(session.Gender))
            {
                logger.LogWarning("Url params don't contain - gender!");
            }
            
            if (string.IsNullOrEmpty(session.Environment))
            {
                logger.LogWarning("Url params don't contain - Environments!");
                session.Environment = FireballConfig.DEFAULT_ENVIRONMENT.ToString();
            }

            if (string.IsNullOrEmpty(session.GameMode))
            {
                logger.LogWarning("Url params don't contain - mode!");
                session.GameMode = FireballConfig.DEFAULT_GAME_MODE.ToString();
            }
            else if (Enum.TryParse(session.GameMode, true, out GameMode mode))
            {
                session.GameMode = mode.ToString();
            }
            else
            {
                logger.LogWarning($"Can't parse - mode! Mode String = {session.GameMode}");
                session.GameMode = FireballConfig.DEFAULT_GAME_MODE.ToString();
            }

            if (Enum.TryParse(session.Environment, true, out Environments environments))
            {
                session.Environment = environments.ToString();
            }
            else
            {
                logger.LogWarning($"Can't parse - Environments! Environments String = {session.Environment}");
                session.Environment = FireballConfig.DEFAULT_ENVIRONMENT.ToString();
            }

            return session;
        }
    }
}                            