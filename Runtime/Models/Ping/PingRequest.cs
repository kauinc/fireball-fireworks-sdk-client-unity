using System;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public class PingRequest : BaseRequest
    {
        private const string REQUEST_NAME = "ping";

        public PingRequest(FireballSession session) : base(REQUEST_NAME, session) { }
    }
}
