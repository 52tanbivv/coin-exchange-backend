﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Funds.Domain.Model.FeeAggregate;
using NHibernate.Linq;
using Spring.Transaction.Interceptor;

namespace CoinExchange.Funds.Infrastructure.Persistence.NHibernate.NHibernate
{
    /// <summary>
    /// Repository for persisteing Fee objects
    /// </summary>
    public class FeeRepository : NHibernateSessionFactory, IFeeRepository
    {
        /// <summary>
        /// Gets the Fee by providing the CurrencyPair name
        /// </summary>
        /// <param name="currencyPair"></param>
        /// <returns></returns>
        [Transaction]
        public List<Fee> GetFeeByCurrencyPair(string currencyPair)
        {
            return CurrentSession.Query<Fee>()
                .Where(x => x.CurrencyPair == currencyPair)
                .AsQueryable()
                .ToList();
        }

        /// <summary>
        /// Gets the fee corresponding to a currency and an amount that will represent the percentage within that range
        /// </summary>
        /// <param name="currencyPair"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Transaction]
        public Fee GetFeeByCurrencyAndAmount(string currencyPair, int amount)
        {
            return CurrentSession
                .QueryOver<Fee>()
                .Where(x => x.CurrencyPair == currencyPair && x.Amount == amount)
                .SingleOrDefault();
        }

        /// <summary>
        /// Gets the fees for every currency pair in the database
        /// </summary>
        /// <returns></returns>
        [Transaction]
        public List<Fee> GetAllFees()
        {
            return CurrentSession.Query<Fee>()
                .AsQueryable()
                .ToList();
        }

        /// <summary>
        /// Gets the Fee by specifying the database primary key ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Transaction]
        public Fee GetFeeById(int id)
        {
            return CurrentSession.QueryOver<Fee>().Where(x => x.Id == id).SingleOrDefault();
        }
    }
}
