using System.Web.Mvc;

namespace PCMS_Application.Areas.LRNR
{
    public class LRNRAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "LRNR";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "LRNR_default",
                "LRNR/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}