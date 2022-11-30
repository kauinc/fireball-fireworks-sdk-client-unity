using System;
using System.Collections.Generic;
using Fireball.Game.Client.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace Fireball.Game.Client.Tools
{
    public static class URLData
    {
        public const string PARAM_NAME_OPERATOR_ID = "operatorId";
        public const string PARAM_NAME_GAME_ID = "gameId";
        public const string PARAM_NAME_PLAYER_ID = "playerId";
        public const string PARAM_NAME_ENVIRONMENT = "environment";
        public const string PARAM_NAME_LANGUAGE = "language";
        public const string PARAM_NAME_CURRENCY = "currency";
        public const string PARAM_NAME_COUNTRY = "country";
        public const string PARAM_NAME_GENDER = "gender";
        public const string PARAM_NAME_TOKEN = "token";
        public const string PARAM_NAME_MODE = "mode";
        public const string PARAM_NAME_WS_SERVER = "messages";
        public const string PARAM_NAME_ROUTER_URL = "router";
        public const string PARAM_NAME_HOME_URL = "home";
        public const string PARAM_NAME_EXTRA = "extra";
        public const string PARAM_NAME_REPLAY = "replay";

        public static FireballSession ParseSessionFromURL(string customUrl = null)
        {
            FireballSession session = new FireballSession();
            Dictionary<string, string> extraDict = new Dictionary<string, string>();
            Dictionary<string, string> paramsDict = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(customUrl))
            {
#if UNITY_WEBGL
                paramsDict = WebLocation.GetSearchParameters();
#elif UNITY_IOS || UNITY_ANDROID
                paramsDict = WebLocation.ParseURLParams(Application.absoluteURL);
#endif
            }
            else
            {
                paramsDict = WebLocation.ParseURLParams(customUrl);
            }

            foreach (string param in paramsDict.Keys)
            {
                if (param.Equals(PARAM_NAME_OPERATOR_ID)) session.OperatorId = paramsDict[param];
                else if (param.Equals(PARAM_NAME_GAME_ID)) session.GameId = paramsDict[param];
                //else if (param.Equals(PARAM_NAME_PLAYER_ID)) session.PlayerId = paramsDict[param];
                else if (param.Equals(PARAM_NAME_ENVIRONMENT)) session.Environment = paramsDict[param];
                else if (param.Equals(PARAM_NAME_LANGUAGE)) session.Language = paramsDict[param];
                else if (param.Equals(PARAM_NAME_CURRENCY)) session.Currency = paramsDict[param];
                else if (param.Equals(PARAM_NAME_COUNTRY)) session.Country = paramsDict[param];
                else if (param.Equals(PARAM_NAME_GENDER)) session.Gender = paramsDict[param];
                else if (param.Equals(PARAM_NAME_TOKEN)) session.Token = paramsDict[param];
                else if (param.Equals(PARAM_NAME_WS_SERVER))
                    session.WsServer = UnityWebRequest.UnEscapeURL(paramsDict[param]);
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
            session.Extra = extraDict;

            return session;
        }

        private static FireballSession ValidateData(FireballSession session)
        {
            IFireballLogger logger = new FireballLogger();
            
            // Check URL params
            if (string.IsNullOrEmpty(session.OperatorId))
            {
                logger.Warning("Url params don't contain - operatorId!");
            }
            
            if (string.IsNullOrEmpty(session.GameId))
            {
                logger.Warning("Url params don't contain - gameId!");
            }

            //if (string.IsNullOrEmpty(session.PlayerId))
            //{
            //    logger.LogWarning("Url params don't contain - playerId!");
            //}

            if (string.IsNullOrEmpty(session.Token))
            {
                logger.Warning("Url params don't contain - token!");
            }

            if (string.IsNullOrEmpty(session.WsServer))
            {
                logger.Warning("Url params don't contain - messages!");
            }
            
            if (string.IsNullOrEmpty(session.Language))
            {
                logger.Warning("Url params don't contain - language!");
                session.Language = FireballConfig.DEFAULT_LANGUAGE_CODE;
            }

            if (string.IsNullOrEmpty(session.Currency))
            {
                logger.Warning("Url params don't contain - currency!");
                session.Currency = FireballConfig.DEFAULT_CURRENCY;
            }

            if (string.IsNullOrEmpty(session.Country))
            {
                logger.Warning("Url params don't contain - country!");
            }
            
            if (string.IsNullOrEmpty(session.Gender))
            {
                logger.Warning("Url params don't contain - gender!");
            }
            
            if (string.IsNullOrEmpty(session.Environment))
            {
                logger.Warning("Url params don't contain - Environments!");
                session.Environment = FireballConfig.DEFAULT_ENVIRONMENT.ToString();
            }

            if (string.IsNullOrEmpty(session.GameMode))
            {
                logger.Warning("Url params don't contain - mode!");
                session.GameMode = FireballConfig.DEFAULT_GAME_MODE.ToString();
            }
            else if (Enum.TryParse(session.GameMode, true, out GameMode mode))
            {
                session.GameMode = mode.ToString();
            }
            else
            {
                logger.Warning($"Can't parse - mode! Mode String = {session.GameMode}");
                session.GameMode = FireballConfig.DEFAULT_GAME_MODE.ToString();
            }

            if (Enum.TryParse(session.Environment, true, out Environments environments))
            {
                session.Environment = environments.ToString();
            }
            else
            {
                logger.Warning($"Can't parse - Environments! Environments String = {session.Environment}");
                session.Environment = FireballConfig.DEFAULT_ENVIRONMENT.ToString();
            }

            return session;
        }
    }
}                            