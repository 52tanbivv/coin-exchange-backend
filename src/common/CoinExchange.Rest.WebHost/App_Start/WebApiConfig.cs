﻿using System.Linq;
using System.Web.Http;
using Common.Logging;

namespace CoinExchange.Rest.WebHost.App_Start
{
    public static class WebApiConfig
    {
        //get current logger
        private static ILog log = LogManager.GetCurrentClassLogger();
         
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //return Json format
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
         
            log.Info("Application Initialized.");
        }
    }
}
