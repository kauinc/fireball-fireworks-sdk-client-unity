using System;
using System.Collections.Generic;
using Fireball.Game.Client.Models;

namespace Fireball.Game.Client.Modules
{
    public class FireballMultiplayer
    {
        private readonly IFireballLogger _logger;
        private readonly Dictionary<string, BroadcastMessageListener> _messageListener = new Dictionary<string, BroadcastMessageListener>();

        public FireballMultiplayer(Fireball fireball, IFireballLogger logger)
        {
            _logger = logger;
            fireball._onBroadcastMessageReceived = BroadcastMessageInvoke;
        }

        public void AddBroadcastListener<T>(string messageName, Action<T> onReceived) where T : BaseMessage
        {
            if (_messageListener.ContainsKey(messageName))
            {
                _logger.Warning($"Multiplayer: rewriting broadcast listener for {messageName}");
            }
            _messageListener[messageName] = new BroadcastMessageListener(messageName, typeof(T), (serverMessage) => onReceived?.Invoke(serverMessage.GetMessage<T>()));
        }

        public void RemoveBroadcastListener(string messageName)
        {
            if (_messageListener.ContainsKey(messageName))
            {
                _messageListener.Remove(messageName);
            }
            else
            {
                _logger.Warning($"Multiplayer: can't remove broadcast listener! Not found listener for {messageName} found!");
            }
        }

        private void BroadcastMessageInvoke(ServerMessage serverMessage)
        {
            if (_messageListener.ContainsKey(serverMessage.Name) && _messageListener[serverMessage.Name] != null)
            {
                _messageListener[serverMessage.Name].MessageAction?.Invoke(serverMessage);
            }
            else
            {
                _logger.Warning($"Multiplayer: no broadcast listener for message {serverMessage.Name}");
            }
        }
    }
}