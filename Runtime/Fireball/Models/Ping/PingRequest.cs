namespace Fireball.Game.Client.Models
{
    public class PingRequest : BaseRequest
    {
        private const string REQUEST_NAME = "ping";

        public PingRequest(FireballSession session) : base(REQUEST_NAME, session) { }
    }
}
