namespace KAU.FireballSDK.Modules
{
    public class WebSocketClose : Jsonable
    {
        public string wsToken;

        public WebSocketClose(string wsToken)
        {
            this.wsToken = wsToken;
        }
    }
}