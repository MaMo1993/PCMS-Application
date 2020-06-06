using System.Web.Mvc;

namespace PCMS_Application.Areas.GURDN
{
    public class GURDNAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "GURDN";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "GURDN_default",
                "GURDN/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}