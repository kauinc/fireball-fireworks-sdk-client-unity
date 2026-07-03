using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Fireball.Game.Client.Tools
{
    [Serializable]
    public class SimpleKeyValue<T>
    {
        public string Key;
        public T Value;
    }

    public static class SimpleKeyValueExtension
    {
        public static Dictionary<string, T> ToDictionary<T>(this List<SimpleKeyValue<T>> simpleKeyValues)
        {
            Dictionary<string, T> dict = new Dictionary<string, T>();
            if (simpleKeyValues != null)
            {
                foreach (var item in simpleKeyValues)
                {
                    dict.Add(item.Key, item.Value);
                }
            }
            return dict;
        }
    }
}
