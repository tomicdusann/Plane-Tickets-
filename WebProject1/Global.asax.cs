using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebProject1.Models;

namespace WebProject1
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles); 
            LetStatusChecker.Start();
        }

        //za sesiju
        protected void Application_PostAuthorizeRequest()
        {
            // Omogućavamo sesije za sve request-ove
            System.Web.HttpContext.Current.SetSessionStateBehavior(
                System.Web.SessionState.SessionStateBehavior.Required);
        }

        protected void Application_End()
        {
            // Zaustavljanje servisa kada se aplikacija gasi
            LetStatusChecker.Stop();
        }
    }
}
