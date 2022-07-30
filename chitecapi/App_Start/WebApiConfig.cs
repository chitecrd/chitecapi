using System.Web.Http;
using System.Net;
using System.Net.Security;

namespace chitecapi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback (Certificates.ValidateRemoteCertificate);
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = RouteParameter.Optional }
            );
        }
    }
}
