using System;
using System.Collections.Generic;
using Fireball.Game.Client.Models;
using Newtonsoft.Json.Linq;

namespace Fireball.Game.Client.Modules
{
    public class FireballMultiplayer
    {
        private readonly Dictionary<string, BroadcastMessageListener> _messageListener = new Dictionary<string, BroadcastMessageListener>();


        public FireballMultiplayer(Fireball fireball)
        {
            fireball._onBroadcastMessageReceived = BroadcastMessageInvoke;
        }

        public void AddBroadcastListener<T>(string messageName, Action<BaseMessage> onReceived) where T : BaseMessage
        {
            var messageType = typeof(T);

            if (!_messageListener.ContainsKey(messageName) || _messageListener[messageName] == null)
            {
                _messageListener[messageName] = new BroadcastMessageListener(messageName, messageType);
            }

            _messageListener[messageName].Add(onReceived);
        }

        public void RemoveBroadcastListener<T>(string messageName, Action<BaseMessage> onReceived) where T : BaseMessage
        {
            var messageType = typeof(T);

            if (_messageListener.ContainsKey(messageName) && _messageListener[messageName].Contains(onReceived))
            {
                _messageListener[messageName].Remove(onReceived);
            }
        }

        private void BroadcastMessageInvoke(string messageName, JToken response)
        {
            if (_messageListener[messageName] != null)
            {
                var listener = _messageListener[messageName];
                var messageObject = response.ToObject(listener.MessageType);

                if (listener.MessageActions != null)
                {
                    foreach (var action in listener.MessageActions)
                    {
                        action?.Invoke(messageObject as BaseMessage);
                    }
                }
            }
        }
    }
}