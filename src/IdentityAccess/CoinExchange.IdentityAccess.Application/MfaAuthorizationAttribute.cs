﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using CoinExchange.IdentityAccess.Domain.Model.Services;
using Newtonsoft.Json.Linq;
using Spring.Context.Support;

namespace CoinExchange.IdentityAccess.Application
{
    /// <summary>
    /// Checks user's authorization subscription and acts accordingly
    /// </summary>
    public class MfaAuthorizationAttribute : ActionFilterAttribute
    {
        // Get Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
              (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IMfaAuthorizationService _mfaAuthorizationService;
        private string _currentAction = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Web.Http.Filters.ActionFilterAttribute"/> class.
        /// </summary>
        public MfaAuthorizationAttribute(string currentAction)
        {
            _mfaAuthorizationService = (IMfaAuthorizationService)ContextRegistry.GetContext().GetObject("MfaAuthorizationService");
            _currentAction = currentAction;
        }

        /// <summary>
        /// Checks Mfa Authorization for a user before the action executes
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            string inputStream = "";
            using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                inputStream = reader.ReadToEnd();
            }

            string mfaCode =
                (string)Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string, object>>(inputStream)["MfaCode"];

            var headers = HttpContext.Current.Request.Headers;
            string authValues = headers.Get("Auth");
            string apiKey = authValues.Split(',')[0];

            Tuple<bool, string> authorizationResponse = _mfaAuthorizationService.AuthorizeAccess(apiKey, _currentAction,
                                                                                                 mfaCode);
            if (authorizationResponse.Item1)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Format("Mfa Code verification Successful"));
                }
                base.OnActionExecuting(actionContext);
            }
            else
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Format("Mfa Code verification failed"));
                }
                actionContext.Response = new HttpResponseMessage();
                JObject jObject = new JObject();
                jObject.Add("Successful", authorizationResponse.Item1);
                jObject.Add("Message", authorizationResponse.Item2);
                actionContext.Response.Content = new StringContent(jObject.ToString());
            }
        }
    }
}
