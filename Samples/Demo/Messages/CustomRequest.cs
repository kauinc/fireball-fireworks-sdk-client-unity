using KAU.FireballSDK;
using KAU.FireballSDK.Models;

namespace KAU.FireballSDK.Demo.Requests
{
    public class CustomRequest : BaseRequest
    {
        public const string REQUEST_NAME = "custom-message";

        public string CustomData;
        
        public CustomRequest(string customData, FireballSession session) : base(REQUEST_NAME, session)
        {
            CustomData = customData;
        }
    }
}