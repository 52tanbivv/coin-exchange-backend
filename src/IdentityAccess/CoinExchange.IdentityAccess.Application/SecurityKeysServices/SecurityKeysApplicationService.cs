﻿using System;
using CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate;

namespace CoinExchange.IdentityAccess.Application.SecurityKeysServices
{
    /// <summary>
    /// Serves operations related to the Digital Signature(API Secret Key pair)
    /// </summary>
    public class SecurityKeysApplicationService : ISecurityKeysApplicationService
    {
        private ISecurityKeysGenerationService _securityKeysGenerationService;

        /// <summary>
        /// Initializes the service for operating operations for the DigitalSignatures
        /// </summary>
        public SecurityKeysApplicationService(ISecurityKeysGenerationService securityKeysGenerationService)
        {
            _securityKeysGenerationService = securityKeysGenerationService;
        }

        /// <summary>
        /// Generates a new API key and Secret Key pair
        /// </summary>
        /// <returns></returns>
        public SecurityKeysPair GetNewKey()
        {
            return _securityKeysGenerationService.GenerateNewSecurityKeys();
        }

        /// <summary>
        /// Set the permissions
        /// </summary>
        public void SetPermission()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validate the given API Key
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public bool ApiKeyValidation(string apiKey)
        {
            // ToDo: Get the API Key from the database
            throw new NotImplementedException();
        }
    }
}
