using System;
using System.Collections.Generic;
using Fireball.Game.Client.Models;

namespace Fireball.Game.Client.Modules
{
    public class BroadcastMessageListener
    {
        public string MessageName;
        public Type MessageType;
        public List<Action<BaseMessage>> MessageActions;

        public BroadcastMessageListener(string name, Type type)
        {
            MessageName = name;
            MessageType = type;
            MessageActions = new List<Action<BaseMessage>>();
        }

        public void Add(Action<BaseMessage> action)
        {
            if (MessageActions == null)
                MessageActions = new List<Action<BaseMessage>>();

            if (!MessageActions.Contains(action))
                MessageActions.Add(action);
        }

        public void Remove(Action<BaseMessage> action)
        {
            if (MessageActions == null)
                MessageActions = new List<Action<BaseMessage>>();

            if (MessageActions.Contains(action))
                MessageActions.Remove(action);
        }
    }
}