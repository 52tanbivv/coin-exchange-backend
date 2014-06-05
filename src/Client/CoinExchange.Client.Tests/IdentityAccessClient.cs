﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CoinExchange.Client.Tests
{
    /// <summary>
    /// Idenetity access client
    /// </summary>
    public class IdentityAccessClient:ApiClient
    {
        public IdentityAccessClient(string baseUrl) : base(baseUrl)
        {
        }

        public string SignUp(string email, string username, string password, string country, TimeZone timeZone, string pgpPublicKey)
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("Email", email);
            jsonObject.Add("Username", username);
            jsonObject.Add("Country", country);
            jsonObject.Add("TimeZone", timeZone.ToString());
            jsonObject.Add("PgpPublicKey", pgpPublicKey);
            jsonObject.Add("Password", password);
            string url = _baseUrl + "/admin/signup";
            return HttpPostRequest(jsonObject, url);
        }

        public string Login(string username, string password)
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("Username", username);
            jsonObject.Add("Password", password);
            string url = _baseUrl + "/admin/login";
            return HttpPostRequest(jsonObject, url);
        }
        
    }
}
