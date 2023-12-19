using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Fireball.Game.Client.Tools
{
    public static class FireballTools
    {
        private static long _requestNum;

        public static long GetNowTimestampMilliSeconds()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        public static string GenerateActionID()
        {
            string actionId = string.Empty;

            try
            {
                actionId = Guid.NewGuid().ToString();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                long timestamp = GetNowTimestampMilliSeconds();
                
                if (_requestNum >= timestamp)
                {
                    _requestNum++;
                }
                else
                {
                    _requestNum = timestamp;
                }

                actionId = _requestNum.ToString();
            }
            
            return actionId;
        }

        public static string GenerateConnectionToken()
        {
            return Guid.NewGuid().ToString();
        }

        public static string EncodeURL(string url)
        {
            return UnityWebRequest.EscapeURL(url);
        }

        public static string DecodeURL(string url)
        {
            return UnityWebRequest.UnEscapeURL(url);
        }

        public static string FormatUrlAndParams(string url, Dictionary<string, string> data = null)
        {
            if (data != null && data.Keys.Count > 0)
            {
                List<string> parameters = new List<string>();
                
                foreach (string param in data.Keys)
                {
                    parameters.Add($"{param}={data[param]}");
                }
                
                string paramsString = string.Join("&", parameters);
                url = $"{url}{(data.Keys.Contains("?") ? "&" : "?")}{paramsString}";
            }
            return url;
        }
    }
}


