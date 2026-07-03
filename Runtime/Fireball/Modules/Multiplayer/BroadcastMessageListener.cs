using System;
using System.Collections.Generic;
using Fireball.Game.Client.Models;

namespace Fireball.Game.Client.Modules
{
    public class BroadcastMessageListener
    {
        public string MessageName;
        public Type MessageType;
        public Action<ServerMessage> MessageAction;

        public BroadcastMessageListener(string name, Type type, Action<ServerMessage> action)
        {
            MessageName = name;
            MessageType = type;
            MessageAction = action;
        }
    }
}