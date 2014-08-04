﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoinExchange.Common.Tests;
using CoinExchange.Funds.Domain.Model.BalanceAggregate;
using CoinExchange.Funds.Domain.Model.CurrencyAggregate;
using CoinExchange.Funds.Domain.Model.DepositAggregate;
using CoinExchange.Funds.Domain.Model.Repositories;
using NUnit.Framework;
using Spring.Context.Support;

namespace CoinExchange.Funds.Infrastucture.NHibernate.IntegrationTests.DatabasePersistenceTests
{
    [TestFixture]
    class BalanceDatabasePersistenceTests
    {
        private DatabaseUtility _databaseUtility;
        private IFundsPersistenceRepository _persistanceRepository;
        private IBalanceRepository _balanceRepository;

        [SetUp]
        public void Setup()
        {
            _balanceRepository = (IBalanceRepository)ContextRegistry.GetContext()["BalanceRepository"];
            _persistanceRepository = (IFundsPersistenceRepository)ContextRegistry.GetContext()["FundsPersistenceRepository"];

            var connection = ConfigurationManager.ConnectionStrings["MySql"].ToString();
            _databaseUtility = new DatabaseUtility(connection);
            _databaseUtility.Create();
            _databaseUtility.Populate();
        }

        [TearDown]
        public void Teardown()
        {
            _databaseUtility.Create();
        }

        [Test]
        public void SaveDepositAddressesAndRetreiveByAccountIdTest_SavesObjectsToDatabase_ChecksIfTheyAreAsExpected()
        {
            Balance balance = new Balance(new Currency("LTC", true), new AccountId("123"), 5000, 4000);

            _persistanceRepository.SaveOrUpdate(balance);

            Balance retrievedDepositAddressList = _balanceRepository.GetBalanceByCurrencyAndAccountId(balance.Currency, balance.AccountId);
            Assert.IsNotNull(retrievedDepositAddressList);

            Assert.AreEqual(balance.AvailableBalance, retrievedDepositAddressList.AvailableBalance);
            Assert.AreEqual(balance.CurrentBalance, retrievedDepositAddressList.CurrentBalance);
            Assert.AreEqual(balance.PendingBalance, retrievedDepositAddressList.PendingBalance);
        }

        [Test]
        public void SavePendingTransactionsTest_SavesObjectsToPendingTransactionsList_ChecksIfTheyAreAsExpected()
        {
            Balance balance = new Balance(new Currency("LTC", true), new AccountId("123"), 5000, 5000);
            _persistanceRepository.SaveOrUpdate(balance);
            bool addPendingTransaction = balance.AddPendingTransaction("withdrawid123", PendingTransactionType.Withdraw, -500);
            Assert.IsTrue(addPendingTransaction);

            _persistanceRepository.SaveOrUpdate(balance);

            Balance retrievedBalance = _balanceRepository.GetBalanceByCurrencyAndAccountId(balance.Currency, balance.AccountId);
            Assert.IsNotNull(retrievedBalance);

            Assert.AreEqual(4500, retrievedBalance.AvailableBalance);
            Assert.AreEqual(5000, retrievedBalance.CurrentBalance);
            Assert.AreEqual(500, retrievedBalance.PendingBalance);

            retrievedBalance.ConfirmPendingTransaction("withdrawid123", PendingTransactionType.Withdraw, -500);

            _persistanceRepository.SaveOrUpdate(retrievedBalance);
            retrievedBalance = _balanceRepository.GetBalanceByCurrencyAndAccountId(balance.Currency, balance.AccountId);
            Assert.IsNotNull(retrievedBalance);
            Assert.AreEqual(4500, retrievedBalance.AvailableBalance);
            Assert.AreEqual(4500, retrievedBalance.CurrentBalance);
            Assert.AreEqual(0, retrievedBalance.PendingBalance);
        }
    }
}
