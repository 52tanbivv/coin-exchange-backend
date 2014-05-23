﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate
{
    /// <summary>
    /// Represents the Permissions for a pair of API Key <-> SecretKey
    /// </summary>
    public class DigitalSignaturePermission
    {
        /// <summary>
        /// parameterized Constructor
        /// </summary>
        /// <param name="securityKeys"></param>
        /// <param name="permissionId"></param>
        /// <param name="isAllowed"></param>
        public DigitalSignaturePermission(DigitalSignature securityKeys, Permission permissionId, bool isAllowed)
        {
            SecurityKeys = securityKeys;
            Permission = permissionId;
            IsAllowed = isAllowed;
        }

        /// <summary>
        /// API Key
        /// </summary>
        public DigitalSignature SecurityKeys { get; private set; }

        /// <summary>
        /// Permission ID
        /// </summary>
        public Permission Permission { get; private set; }

        /// <summary>
        /// IsAllowed
        /// </summary>
        public bool IsAllowed { get; private set; }
    }
}
