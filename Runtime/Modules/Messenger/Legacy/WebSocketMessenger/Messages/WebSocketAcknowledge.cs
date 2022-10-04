namespace Fireball.Game.Client.Modules
{
    [System.Serializable]
    public class WebSocketAcknowledge : Jsonable
    {
        public string name = "acknowledge-message";
        public string wsMessageId = null;
        public WebSocketAcknowledge(string id) { wsMessageId = id; }
    }
}
