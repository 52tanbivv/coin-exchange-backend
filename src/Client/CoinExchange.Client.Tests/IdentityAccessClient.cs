﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public string ActivateUser(string username, string password,string activationKey)
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("Username", username);
            jsonObject.Add("Password", password);
            jsonObject.Add("ActivationKey", activationKey);
            string url = _baseUrl + "/admin/user/activate";
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

        public string KeyPairList()
        {
            string url = _baseUrl + "/private/user/api/list";
            return HttpGetRequest(url);
        }

        public string ListPermissions()
        {
            string url = _baseUrl + "/private/user/api/permissions";
            return HttpGetRequest(url);
        }

        public string CreateKey(string keyDescription, PermissionRepresentation[] permissions)
        {
            SecuritykeysPersmission persmission=new SecuritykeysPersmission("","","",false,false,false,keyDescription,permissions);
            string url = _baseUrl + "/private/user/api/create";
            return HttpPostRequest(persmission, url);
        }

        public string Logout()
        {
            string url = _baseUrl + "/private/admin/logout";
            return HttpGetRequest(url);
        }

        public string ApplyForTierLevel1(string fullName, string dateOfBirth, string phoneNumber)
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("FullName", fullName);
            jsonObject.Add("DateOfBirth", dateOfBirth);
            jsonObject.Add("PhoneNumber", phoneNumber);
            string url = _baseUrl + "/private/user/tier1";
            return HttpPostRequest(jsonObject, url);
        }

        public string ApplyForTierLevel2(string addressLin1, string addressLine2, string addressLine3, string state,
            string city, string zipCode)
        {
            JObject jsonObject = new JObject();
            jsonObject.Add("AddressLine1", addressLin1);
            jsonObject.Add("AddressLine2", addressLine2);
            jsonObject.Add("AddressLine3", addressLine3);
            jsonObject.Add("State", state);
            jsonObject.Add("City", city);
            jsonObject.Add("ZipCode", zipCode);
            string url = _baseUrl + "/private/user/tier2";
            return HttpPostRequest(jsonObject, url);
        }
    }
}
