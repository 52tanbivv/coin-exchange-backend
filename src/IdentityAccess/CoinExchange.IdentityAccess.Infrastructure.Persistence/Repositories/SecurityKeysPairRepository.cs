﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.IdentityAccess.Domain.Model.Repositories;
using CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate;
using Spring.Stereotype;
using Spring.Transaction.Interceptor;

namespace CoinExchange.IdentityAccess.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository for DigitalSignatureInfo
    /// </summary>
    [Repository]
    public class SecurityKeysPairRepository : NHibernateSessionFactory, ISecurityKeysRepository
    {
        [Transaction(ReadOnly = true)]
        public SecurityKeysPair GetByKeyDescription(string keyDescription)
        {
            return CurrentSession.Get<SecurityKeysPair>(keyDescription);
        }

        [Transaction(ReadOnly = true)]
        public SecurityKeysPair GetByApiKey(string apiKey)
        {
            return
                CurrentSession.QueryOver<SecurityKeysPair>()
                    .Where(x => x.ApiKey.Value == apiKey)
                    .SingleOrDefault();
        }
    }
}
