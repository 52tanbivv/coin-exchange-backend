﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Common.Tests;
using CoinExchange.IdentityAccess.Application.RegistrationServices;
using CoinExchange.IdentityAccess.Application.RegistrationServices.Commands;
using CoinExchange.IdentityAccess.Domain.Model.Repositories;
using CoinExchange.IdentityAccess.Domain.Model.UserAggregate;
using CoinExchange.IdentityAccess.Infrastructure.Persistence.Repositories;
using CoinExchange.IdentityAccess.Infrastructure.Services;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

namespace CoinExchange.IdentityAccess.Application.Tests
{
    [TestFixture]
    public class RegistrationApplicationServiceUnitTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        [Category("Unit")]
        public void UserCreatedTest_ChecksIfTheRegistrationServiceCreatesAUserAsExpected()
        {
            IPersistenceRepository persistenceRepository = new MockPersistenceRepository(false);
            RegistrationApplicationService registrationApplicationService = new RegistrationApplicationService(persistenceRepository, 
                new PasswordEncryptionService(), new ActivationKeyGenerationService());

            string activationKey = registrationApplicationService.CreateAccount(
                new SignupUserCommand("testdriven@agile.com", "iamnotmartinfowler", "butiamjohnskeet", "ProgrammingNation", 
                    TimeZone.CurrentTimeZone, ""));

            Assert.IsNotNull(activationKey);
            Assert.IsFalse(string.IsNullOrEmpty(activationKey));
        }

        [Test]
        [Category("Unit")]
        public void EmailNotProvidedTest_TestsIfNewUserIsNotCreatedWhenEmailIsNotGiven_ChecksActivationKeyisNotReturnedToConfirm()
        {
            IPersistenceRepository persistenceRepository = new MockPersistenceRepository(false);
            RegistrationApplicationService registrationApplicationService = new RegistrationApplicationService(persistenceRepository,
                new PasswordEncryptionService(), new ActivationKeyGenerationService());

            string activationKey = registrationApplicationService.CreateAccount(
                new SignupUserCommand("", "agilegeek", "iammartinfowler", "ProgrammingNation",
                    TimeZone.CurrentTimeZone, ""));

            Assert.IsNull(activationKey);
        }

        [Test]
        [Category("Unit")]
        public void UsernameNotProvidedTest_TestsIfNewUserIsNotCreatedWhenUsernameIsNotGiven_ChecksActivationKeyisNotReturnedToConfirm()
        {
            IPersistenceRepository persistenceRepository = new MockPersistenceRepository(false);
            RegistrationApplicationService registrationApplicationService = new RegistrationApplicationService(persistenceRepository,
                new PasswordEncryptionService(), new ActivationKeyGenerationService());

            string activationKey = registrationApplicationService.CreateAccount(
                new SignupUserCommand("testdriven@agile.com", null, "iammartinfowler", "ProgrammingNation",
                    TimeZone.CurrentTimeZone, ""));

            Assert.IsNull(activationKey);
        }

        [Test]
        [Category("Unit")]
        public void PasswordNotProvidedTest_TestsIfNewUserIsNotCreatedWhenPasswordIsNotGiven_ChecksActivationKeyisNotReturnedToConfirm()
        {
            IPersistenceRepository persistenceRepository = new MockPersistenceRepository(false);
            RegistrationApplicationService registrationApplicationService = new RegistrationApplicationService(persistenceRepository,
                new PasswordEncryptionService(), new ActivationKeyGenerationService());

            string activationKey = registrationApplicationService.CreateAccount(
                new SignupUserCommand("testdriven@agile.com", "iamnotmartinfowler", "", "ProgrammingNation",
                    TimeZone.CurrentTimeZone, ""));

            Assert.IsNull(activationKey);
        }

        [Test]
        [Category("Unit")]
        public void DatabaseMockSaveFailTest_TestsIfUserIsNotSavedInDatabaseExceptionIsRaisedFromTheMockClass_HandlesTheExceptionToConfirm()
        {
            // Provide true to the Mock class  so that it raises exception when SaveUpdate method is called inside it
            IPersistenceRepository persistenceRepository = new MockPersistenceRepository(true);
            RegistrationApplicationService registrationApplicationService = new RegistrationApplicationService(persistenceRepository,
                new PasswordEncryptionService(), new ActivationKeyGenerationService());

            bool exceptionRaised = false;
            try
            {
                string activationKey = registrationApplicationService.CreateAccount(new SignupUserCommand(
                    "testdriven@agile.com", "iamnotmartinfowler", "butiamjacksparrow", "PirateNation", 
                    TimeZone.CurrentTimeZone, ""));
                Assert.IsNull(activationKey);
            }
            catch (Exception e)
            {
                exceptionRaised = true;
            }

            Assert.IsTrue(exceptionRaised);
        }
    }
}
