using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FuzzyMsc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AutofacConfig.Register();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Z.EntityFramework.Extensions.LicenseManager.AddLicense("37;300-LIVECYCLE", "79413390F5310156A2D165AC444631AF");
            Z.EntityFramework.Extensions.LicenseManager.AddLicense("6;100-LIVECYCLE", "20028999CE1021FF66CD379917DF51AE");

           

        }
    }
}
