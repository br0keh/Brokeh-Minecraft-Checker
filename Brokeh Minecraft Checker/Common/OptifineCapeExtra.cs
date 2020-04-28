using xNet;

namespace Brokeh_Minecraft_Checker.Common
{
    public class OptifineCapeExtra : AccountExtra
    {
        private const string UrlEndpoint = "http://s.optifine.net/capes/";

        public OptifineCapeExtra() : base("Optifine Cape")
        {
        }

        public override bool CheckExtra(Account account)
        {
            using (var request = new HttpRequest())
            {
                try
                {
                    var cookies = new CookieDictionary();
                    request.Cookies = cookies;
                    request.IgnoreProtocolErrors = true;
                    request.ConnectTimeout = 10 * 1000;
                    request.AllowAutoRedirect = true;
                    request.KeepAlive = true;

                    var response = request.Get(UrlEndpoint + account.Username + ".png");

                    return response.StatusCode == HttpStatusCode.OK;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}