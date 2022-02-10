using Newtonsoft.Json;

namespace KAU.FireballSDK.Modules
{
    public class Jsonable : IJsonable
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}
