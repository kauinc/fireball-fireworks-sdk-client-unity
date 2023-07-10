using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class BaseResponse : BaseMessage
    {
        public Dictionary<string, object> client_side;

        public T GetClientVariables<T>() where T : class
        {
            T vars = null;
            try
            {
                var varsJson = JsonConvert.SerializeObject(this.client_side);
                vars = JsonConvert.DeserializeObject<T>(varsJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Fireball] Can't serialize Client Variables to type = {typeof(T).Name}. Error: {e.Message}");
            }
            return vars;
        }
    }

    public class ResponseMessageWrapper<T> where T : BaseResponse
    {
        public string ActionId;
        public string WsMessageId;
        public T Message;
    }
}
