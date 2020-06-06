using System.Web.Mvc;

namespace PCMS_Application.Areas.SCHL
{
    public class SCHLAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SCHL";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SCHL_default",
                "SCHL/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}