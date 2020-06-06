using System.Web.Mvc;

namespace PCMS_Application.Areas.PCMS_ADMN
{
    public class PCMS_ADMNAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PCMS_ADMN";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PCMS_ADMN_default",
                "PCMS_ADMN/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}