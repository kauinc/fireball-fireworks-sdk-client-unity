using System;
using KAU.FireballSDK.Modules;

namespace KAU.FireballSDK.Models
{
    [Serializable]
    public class BaseResponse : Jsonable
    {
        // required fields
        public string name;
        public string actionId;
        public string logId;
    }
}
