namespace Fireball.Game.Client.Modules
{
    public class FireballGCIEvent
    {
        public string name = null;
        public object value = null;

        public FireballGCIEvent(string name, object value = null)
        {
            this.name = name;
            this.value = value;
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
