﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoinExchange.Common.Tests;
using CoinExchange.Funds.Domain.Model.DepositAggregate;
using CoinExchange.Funds.Domain.Model.Repositories;
using NUnit.Framework;
using Spring.Context.Support;

namespace CoinExchange.Funds.Infrastucture.NHibernate.IntegrationTests
{
    [TestFixture]
    class DepositDatabasePersistenceTests //: AbstractConfiguration
    {
        private DatabaseUtility _databaseUtility;

        [SetUp]
        public void Setup()
        {
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

        private IFundsPersistenceRepository _persistanceRepository;
        private IDepositRepository _depositRepository;

        /// <summary>
        /// Spring's Injection using FundsAbstractConfiguration class
        /// </summary>
        public IDepositRepository DepositRepository
        {
            set { _depositRepository = value; }
        }

        /// <summary>
        /// Spring's Injection using FundsAbstractConfiguration class
        /// </summary>
        public IFundsPersistenceRepository Persistance
        {
            set { _persistanceRepository = value; }
        }

        [Test]
        public void SaveDepositAndRetreiveByIdTest_SavesAnObjectToDatabaseAndManipulatesIt_ChecksIfItIsUpdatedAsExpected()
        {
            _depositRepository = (IDepositRepository)ContextRegistry.GetContext()["DepositRepository"];
            _persistanceRepository = (IFundsPersistenceRepository)ContextRegistry.GetContext()["FundsPersistenceRepository"];
            Deposit deposit = new Deposit(new Currency("LTC", true), "1234", DateTime.Now, "New", 2000, 0.005, TransactionStatus.Pending,
                new AccountId("123"));

            _persistanceRepository.SaveOrUpdate(deposit);

            Deposit retrievedDeposit = _depositRepository.GetDepositByDepositId("1234");
            Assert.IsNotNull(retrievedDeposit);
            int id = retrievedDeposit.Id;
            retrievedDeposit.Amount = 777;
            _persistanceRepository.SaveOrUpdate(retrievedDeposit);

            retrievedDeposit = _depositRepository.GetDepositById(id);
            Assert.AreEqual(deposit.Currency.Name, retrievedDeposit.Currency.Name);
            Assert.AreEqual(deposit.DepositId, retrievedDeposit.DepositId);
            Assert.AreEqual(deposit.Type, retrievedDeposit.Type);
            Assert.AreEqual(777, retrievedDeposit.Amount);
            Assert.AreEqual(deposit.Fee, retrievedDeposit.Fee);
            Assert.AreEqual(deposit.Status, retrievedDeposit.Status);
            Assert.AreEqual(deposit.AccountId.Value, retrievedDeposit.AccountId.Value);
        }

        [Test]
        public void SaveDepositAndRetreiveByDepositIdTest_SavesAnObjectToDatabaseAndManipulatesIt_ChecksIfItIsUpdatedAsExpected()
        {
            _depositRepository = (IDepositRepository)ContextRegistry.GetContext()["DepositRepository"];
            _persistanceRepository = (IFundsPersistenceRepository)ContextRegistry.GetContext()["FundsPersistenceRepository"];
            Deposit deposit = new Deposit(new Currency("LTC", true), "1234", DateTime.Now, "New", 2000, 0.005, TransactionStatus.Pending, 
                new AccountId("123"));

            _persistanceRepository.SaveOrUpdate(deposit);

            Deposit retrievedDeposit = _depositRepository.GetDepositByDepositId("1234");
            Assert.IsNotNull(retrievedDeposit);
            retrievedDeposit.Amount = 777;
            _persistanceRepository.SaveOrUpdate(retrievedDeposit);

            retrievedDeposit = _depositRepository.GetDepositByDepositId("1234");
            Assert.AreEqual(deposit.Currency.Name, retrievedDeposit.Currency.Name);
            Assert.AreEqual(deposit.DepositId, retrievedDeposit.DepositId);
            Assert.AreEqual(deposit.Type, retrievedDeposit.Type);
            Assert.AreEqual(777, retrievedDeposit.Amount);
            Assert.AreEqual(deposit.Fee, retrievedDeposit.Fee);
            Assert.AreEqual(deposit.Status, retrievedDeposit.Status);
            Assert.AreEqual(deposit.AccountId.Value, retrievedDeposit.AccountId.Value);
        }

        [Test]
        public void SaveDepositAndRetreiveByCurrencyNameTest_SavesAnObjectToDatabaseAndManipulatesIt_ChecksIfItIsUpdatedAsExpected()
        {
            _depositRepository = (IDepositRepository)ContextRegistry.GetContext()["DepositRepository"];
            _persistanceRepository = (IFundsPersistenceRepository)ContextRegistry.GetContext()["FundsPersistenceRepository"];
            Deposit deposit = new Deposit(new Currency("LTC", true), "1234", DateTime.Now, "New", 2000, 0.005, TransactionStatus.Pending,
                new AccountId("123"));

            _persistanceRepository.SaveOrUpdate(deposit);

            List<Deposit> retrievedDeposits = _depositRepository.GetDepositByCurrencyName("LTC");
            Assert.IsNotNull(retrievedDeposits);
            retrievedDeposits[0].Amount = 777;
            _persistanceRepository.SaveOrUpdate(retrievedDeposits[0]);

            retrievedDeposits = _depositRepository.GetDepositByCurrencyName("LTC");
            Assert.AreEqual(deposit.Currency.Name, retrievedDeposits[0].Currency.Name);
            Assert.AreEqual(deposit.DepositId, retrievedDeposits[0].DepositId);
            Assert.AreEqual(deposit.Type, retrievedDeposits[0].Type);
            Assert.AreEqual(777, retrievedDeposits[0].Amount);
            Assert.AreEqual(deposit.Fee, retrievedDeposits[0].Fee);
            Assert.AreEqual(deposit.Status, retrievedDeposits[0].Status);
            Assert.AreEqual(deposit.AccountId.Value, retrievedDeposits[0].AccountId.Value);
        }

        [Test] public void SaveDepositsAndRetreiveByAccountIdTest_SavesMultipleObjectsToDatabase_ChecksIfTheyAreAsExpected()
        {
            _depositRepository = (IDepositRepository)ContextRegistry.GetContext()["DepositRepository"];
            _persistanceRepository = (IFundsPersistenceRepository)ContextRegistry.GetContext()["FundsPersistenceRepository"];
            Deposit deposit = new Deposit(new Currency("LTC", true), "1234", DateTime.Now, "New", 2000, 0.005, TransactionStatus.Pending,
                new AccountId("123"));

            _persistanceRepository.SaveOrUpdate(deposit);

            Deposit deposit2 = new Deposit(new Currency("BTC", true), "123", DateTime.Now, "New", 1000, 0.010, TransactionStatus.Pending,
                new AccountId("123"));
            Thread.Sleep(500);

            _persistanceRepository.SaveOrUpdate(deposit2);

            List<Deposit> retrievedDepositList = _depositRepository.GetDepositByAccountId(new AccountId("123"));
            Assert.IsNotNull(retrievedDepositList);
            Assert.AreEqual(2, retrievedDepositList.Count);

            Assert.AreEqual(deposit.Currency.Name, retrievedDepositList[0].Currency.Name);
            Assert.AreEqual(deposit.DepositId, retrievedDepositList[0].DepositId);
            Assert.AreEqual(deposit.Type, retrievedDepositList[0].Type);
            Assert.AreEqual(deposit.Amount, retrievedDepositList[0].Amount);
            Assert.AreEqual(deposit.Fee, retrievedDepositList[0].Fee);
            Assert.AreEqual(deposit.Status, retrievedDepositList[0].Status);
            Assert.AreEqual(deposit.AccountId.Value, retrievedDepositList[0].AccountId.Value);

            Assert.AreEqual(deposit2.Currency.Name, retrievedDepositList[1].Currency.Name);
            Assert.AreEqual(deposit2.DepositId, retrievedDepositList[1].DepositId);
            Assert.AreEqual(deposit2.Type, retrievedDepositList[1].Type);
            Assert.AreEqual(deposit2.Amount, retrievedDepositList[1].Amount);
            Assert.AreEqual(deposit2.Fee, retrievedDepositList[1].Fee);
            Assert.AreEqual(deposit2.Status, retrievedDepositList[1].Status);
            Assert.AreEqual(deposit2.AccountId.Value, retrievedDepositList[1].AccountId.Value);
        }
    }
}
