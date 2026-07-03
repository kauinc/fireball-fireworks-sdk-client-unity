using System;

namespace Fireball.Game.Client.Modules
{
    [Serializable]
    public class SignalRMessageData
    {
        public string Name;
        public string ActionId;
        public string WsMessageId;
    }
}
