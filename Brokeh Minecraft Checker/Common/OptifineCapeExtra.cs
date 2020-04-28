using xNet;

namespace Brokeh_Minecraft_Checker.Common
{
    public class OptifineCapeExtra : AccountExtra
    {
        private const string UrlEndpoint = "http://s.optifine.net/capes/";
        
        public OptifineCapeExtra() : base("Optifine Cape")
        {
        }

        public override bool CheckExtra(HttpRequest request, Account account)
        {

            return false;
        }
    }
}