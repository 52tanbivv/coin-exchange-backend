﻿using System;

namespace CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate
{
    /// <summary>
    /// Digital Signature Info(Api key and Secret key)
    /// </summary>
    public class SecurityKeysPair
    {
        #region Private Fields

        private ApiKey _apiKey;
        private SecretKey _secretKey;
        private PermissionsList _permissionsList;

        #endregion Private Fields

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SecurityKeysPair()
        {
            _permissionsList = new PermissionsList();
        }

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        public SecurityKeysPair(ApiKey apiKey, SecretKey secretKey)
        {
            _apiKey = apiKey;
            _secretKey = secretKey;
            _permissionsList = new PermissionsList();
        }

        /// <summary>
        /// Parameterized Constructor
        /// </summary>
        /// <param name="keyDescription"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="userName"></param>
        /// <param name="expirationDate"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="lastModified"></param>
        /// <param name="systemGenerated"></param>
        public SecurityKeysPair(string keyDescription, ApiKey apiKey, SecretKey secretKey, string userName, DateTime expirationDate, DateTime startDate, DateTime endDate, DateTime lastModified, bool systemGenerated)
        {
            KeyDescription = keyDescription;
            _apiKey = apiKey;
            _secretKey = secretKey;
            UserName = userName;
            ExpirationDate = expirationDate;
            StartDate = startDate;
            EndDate = endDate;
            LastModified = lastModified;
            SystemGenerated = systemGenerated;

            _permissionsList = new PermissionsList();
        }

        #region Methods

        /// <summary>
        /// Adds the permission to the given list of permissions
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool AddPermission(SecurityKeysPermission permission)
        {
            _permissionsList.AddPermission(permission);
            return true;
        }

        /// <summary>
        /// Removes the given permission from the allowed permision for this Digital Signature
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool RemovePermission(SecurityKeysPermission permission)
        {
            _permissionsList.RemoveTierStatus(permission);
            return true;
        }

        /// <summary>
        /// Change the value of the API Key
        /// </summary>
        /// <param name="apiKey"> </param>
        /// <returns></returns>
        public bool ChangeApiKeyValue(ApiKey apiKey)
        {
            if (IsApiKeyValid(apiKey))
            {
                _apiKey = apiKey;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Change the value of the Secret Key
        /// </summary>
        /// <param name="secretKey"> </param>
        /// <returns></returns>
        public bool ChangeSecretKeyValue(SecretKey secretKey)
        {
            if (IsSecretKeyValid(secretKey))
            {
                _secretKey = secretKey;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Verifies if the given API Key is valid or not
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        private bool IsApiKeyValid(ApiKey apiKey)
        {
            if (apiKey != null && !string.IsNullOrEmpty(apiKey.Value))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Verifies if the given Secret Key is valid or not
        /// </summary>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        private bool IsSecretKeyValid(SecretKey secretKey)
        {
            if (secretKey!= null && !string.IsNullOrEmpty(secretKey.Value))
            {
                return true;
            }
            return false;
        }

        #endregion Methods

        /// <summary>
        /// Key Description
        /// </summary>
        public string KeyDescription
        {
            get; private set;
        }

        /// <summary>
        /// API Key
        /// </summary>
        public ApiKey ApiKey { get { return _apiKey; } set { _apiKey = value; } }

        /// <summary>
        /// Secret Key
        /// </summary>
        public SecretKey SecretKey { get { return _secretKey; } set { _secretKey = value; } }

        /// <summary>
        /// Username
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Expiration Date
        /// </summary>
        public DateTime ExpirationDate { get; private set; }

        /// <summary>
        /// StartDate
        /// </summary>
        public DateTime StartDate { get; private set; }
        
        /// <summary>
        /// EndDate
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// LastModified
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// SystemGenerated
        /// </summary>
        public bool SystemGenerated { get; private set; }

        /// <summary>
        /// List of Permissions granted to this Security Keys Pair
        /// </summary>
        public PermissionsList PermissionList { get { return _permissionsList; } private set { _permissionsList = value; } }
    }
}
