using System.Web.Mvc;

namespace PCMS_Application.Areas.TCHR
{
    public class TCHRAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TCHR";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "TCHR_default",
                "TCHR/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}