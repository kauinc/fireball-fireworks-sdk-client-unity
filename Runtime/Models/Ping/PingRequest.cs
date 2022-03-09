using System;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public class PingRequest : BaseRequest
    {
        private const string REQUEST_NAME = "ping";

        public PingRequest(FireballSession session, string customActionID = null) 
            : base(REQUEST_NAME, session, customActionID) { }
    }
}
