﻿using System.Collections.Generic;
using System.Globalization;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Fireball.Game.Client.Modules
{
    public class WebLocation
    {
        /*
        * https://some.domain.com:port/path/to/resource/resourceName.html?name=Foobar&id=3#HashString
        * | #1 |  |     #2      | |#3||          #4                     ||      #5       ||    #6   |
        *         |        #7        |
        * |            #8            |
        * |                                          #9                                             |
        * #1: location.protocol  "https:"
        * #2: location.hostname  "some.domain.com"
        * #3: location.port      "port"
        * #4: location.pathname  "/path/to/resource/resourceName.html"
        * #5: location.search    "?name=Foobar&id=3"
        * #6: location.hash      "#HashString"
        * 
        * #7: location.host      "some.domain.com:port"
        * #8: location.origin    "https://some.domain.com:port"
        * #9: location.href      "*full URL*"
        */

        private static char[] _splitChars = new char[] { '&' };

#if UNITY_WEBGL && !UNITY_EDITOR

        // REAL DATA FOR UNITY WEBGL 
        [DllImport("__Internal")] private static extern string location_protocol();
        [DllImport("__Internal")] private static extern string location_hostname();
        [DllImport("__Internal")] private static extern string location_port();
        [DllImport("__Internal")] private static extern string location_pathname();
        [DllImport("__Internal")] private static extern string location_search();
        [DllImport("__Internal")] private static extern string location_hash();
        [DllImport("__Internal")] private static extern string location_host();
        [DllImport("__Internal")] private static extern string location_origin();
        [DllImport("__Internal")] private static extern string location_href();

        [DllImport("__Internal")] private static extern void location_set_search(string searchString);
        [DllImport("__Internal")] private static extern void location_set_hash(string hashString);
#else

        // TEST DATA FOR UNITY EDITOR
        private static string location_protocol() => "https";
        private static string location_hostname() => "editor.unity.com";
        private static string location_port() => "80";
        private static string location_pathname() => "index.html";
        private static string location_search() => "?platform=editor";
        private static string location_hash() => "#nohash";
        private static string location_host() => location_hostname() + (string.IsNullOrEmpty(location_port()) ? "" : (":" + location_port()));
        private static string location_origin() => location_protocol() + "//" + location_host();
        private static string location_href() => location_origin() + location_pathname() + location_search() + location_hash();
        private static void location_set_search(string searchString) { }
        private static void location_set_hash(string hashString) { }

#endif

        public static string Protocol { get { return location_protocol(); } }
        public static string Hostname { get { return location_hostname(); } }
        public static string Port { get { return location_port(); } }
        public static string Pathname { get { return location_pathname(); } }
        public static string Search { get { return location_search(); } set { location_set_search(value); } }
        public static string Hash { get { return location_hash(); } set { location_set_hash(value); } }

        public static string Host { get { return location_host(); } }
        public static string Origin { get { return location_origin(); } }
        public static string Href { get { return location_href(); } }


        public static Dictionary<string, string> ParseURLParams(string urlString)
        {
            if (string.IsNullOrEmpty(urlString))
                return new Dictionary<string, string>();

            // skip "?" / "#" and split parameters at "&"
            if (urlString.Contains("?"))
            {
                urlString = urlString.Split('?')[1];
            }

            if (urlString.Contains("#"))
            {
                urlString = urlString.Split('#')[0];
            }

            var parameters = urlString.Split(_splitChars);
            var result = new Dictionary<string, string>(parameters.Length);
            foreach (var p in parameters)
            {
                int pos = p.IndexOf('=');
                if (pos > 0)
                {
                    result[p.Substring(0, pos)] = p.Substring(pos + 1);
                }
                else
                {
                    result[p] = "";
                }
            }
            return result;
        }

        public static Dictionary<string, string> GetSearchParameters()
        {
            return ParseURLParams(Search);
        }

        public static Dictionary<string, string> GetHashParameters()
        {
            return ParseURLParams(Hash);
        }
    }

    public static class DictionaryStringStringExt
    {
        public static double GetDouble(this Dictionary<string, string> dict, string key, double defaultValue)
        {
            string str;
            if (!dict.TryGetValue(key, out str))
                return defaultValue;

            double val;
            if (!double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                return defaultValue;

            return val;
        }
        public static int GetInt(this Dictionary<string, string> dict, string key, int defaultValue)
        {
            string str;
            if (!dict.TryGetValue(key, out str))
                return defaultValue;

            int val;
            if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
                return defaultValue;

            return val;
        }
        public static bool GetValueSafe(this Dictionary<string, string> dict, string key, string defaultValue, out string returnValue)
        {
            if (dict.ContainsKey(key))
            {
                returnValue = dict[key];
                return true;
            }
            returnValue = defaultValue;
            return false;
        }
    }
}