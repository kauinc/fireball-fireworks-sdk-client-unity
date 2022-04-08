using KAU.FireballSDK.Models;

namespace KAU.FireballSDK.Demo.Requests
{
    public class CustomResponse : BaseResponse
    {
        public const string RESPONSE_NAME = "custom-response";

        public string CustomData;

        public CustomResponse()
        {
            name = RESPONSE_NAME;
        }
    }
}