﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate;

namespace CoinExchange.IdentityAccess.Application.SecurityKeysServices
{
    /// <summary>
    /// VO to get permission info from user
    /// </summary>
    public class SecurityKeyPermissionsRepresentation
    {
        public Permission Permission { get; private set; }
        public bool Allowed { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="allowed"></param>
        /// <param name="permission"></param>
        public SecurityKeyPermissionsRepresentation(bool allowed, Permission permission)
        {
            Allowed = allowed;
            Permission = permission;
        }
    }
}
