using Newtonsoft.Json;

namespace Fireball.Game.Client.Modules
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
