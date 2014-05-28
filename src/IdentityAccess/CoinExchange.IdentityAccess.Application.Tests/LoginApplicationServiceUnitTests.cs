﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Common.Tests;
using CoinExchange.IdentityAccess.Application.AccessControlServices;
using CoinExchange.IdentityAccess.Application.AccessControlServices.Commands;
using CoinExchange.IdentityAccess.Application.SecurityKeysServices;
using CoinExchange.IdentityAccess.Domain.Model.Repositories;
using CoinExchange.IdentityAccess.Domain.Model.SecurityKeysAggregate;
using CoinExchange.IdentityAccess.Infrastructure.Persistence.Repositories;
using CoinExchange.IdentityAccess.Infrastructure.Services;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using CoinExchange.IdentityAccess.Domain.Model.UserAggregate;

namespace CoinExchange.IdentityAccess.Application.Tests
{
    [TestFixture]
    public class LoginApplicationServiceUnitTests
    {
        private IApplicationContext _applicationContext;
        private DatabaseUtility _databaseUtility;

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
        public void LoginSuccessfulTest_ChecksIfTheSecurityKeysAreProperlyReturnedWhileLoggingIn_VerifiesTheReturnedKeysToConfirm()
        {
            IUserRepository userRepository = new MockUserRepository();
            IIdentityAccessPersistenceRepository persistRepository = new MockPersistenceRepository(false);
            ISecurityKeysApplicationService securityKeysApplicationService = new SecurityKeysApplicationService(new SecurityKeysGenerationService(),
                persistRepository,null);
            IPasswordEncryptionService passwordEncryptionService = new PasswordEncryptionService();
            ILoginApplicationService loginApplicationService = new LoginApplicationService(userRepository, passwordEncryptionService, 
                securityKeysApplicationService);

            string enteredPassword = "whereistheJoker";
            User user = new User("batman@gotham.com", "brucewayne", passwordEncryptionService.EncryptPassword(enteredPassword),
                "Ninja County", TimeZone.CurrentTimeZone, "", "");
            user.AutoLogout = new TimeSpan(0, 0, 0, 60);
            // Add this user to the MockUserRepository
            (userRepository as MockUserRepository).AddUser(user);
            UserValidationEssentials userValidationEssentials = loginApplicationService.Login(
                new LoginCommand("brucewayne", enteredPassword));

            Assert.IsNotNull(userValidationEssentials);
            Assert.IsNotNull(userValidationEssentials.ApiKey);
            Assert.IsNotNull(userValidationEssentials.SecretKey);
            Assert.AreEqual(userValidationEssentials.SessionLogoutTime, user.AutoLogout);
        }

        [Test]
        [Category("Unit")]
        public void LoginfailDueToIncorrectUsernameTest_MakesSureLoginFailsInCaseOfWrongUsername_VerifiesTheReturnedNullResultToConfirm()
        {
            IUserRepository userRepository = new MockUserRepository();
            IIdentityAccessPersistenceRepository persistRepository = new MockPersistenceRepository(false);
            ISecurityKeysApplicationService securityKeysApplicationService = new SecurityKeysApplicationService(new SecurityKeysGenerationService(),
                persistRepository,null);
            IPasswordEncryptionService passwordEncryptionService = new PasswordEncryptionService();
            ILoginApplicationService loginApplicationService = new LoginApplicationService(userRepository, passwordEncryptionService,
                securityKeysApplicationService);

            string enteredPassword = "whereistheJoker";
            User user = new User("batman@gotham.com", "brucewayne", passwordEncryptionService.EncryptPassword(enteredPassword),
                "Ninja County", TimeZone.CurrentTimeZone, "", "");
            user.AutoLogout = new TimeSpan(0, 0, 0, 60);
            // Add this user to the MockUserRepository
            (userRepository as MockUserRepository).AddUser(user);

            bool exceptionRaised = false;
            try
            {
                loginApplicationService.Login(
                    new LoginCommand("alfred", "whereisthejoker"));
            }
            catch (InvalidCredentialException e)
            {
                exceptionRaised = true;
            }

            Assert.IsTrue(exceptionRaised);
        }

        [Test]
        [Category("Unit")]
        public void LoginfailDueToIncorrectPasswordTest_MakesSureLoginFailsInCaseOfWrongPassword_VerifiesTheReturnedNullResultToConfirm()
        {
            IUserRepository userRepository = new MockUserRepository();
            IIdentityAccessPersistenceRepository persistRepository = new MockPersistenceRepository(false);
            ISecurityKeysApplicationService securityKeysApplicationService = new SecurityKeysApplicationService(new SecurityKeysGenerationService(),
                persistRepository,null);
            IPasswordEncryptionService passwordEncryptionService = new PasswordEncryptionService();
            ILoginApplicationService loginApplicationService = new LoginApplicationService(userRepository, passwordEncryptionService,
                securityKeysApplicationService);
            string enteredPassword = "whereistheJoker";
            User user = new User("batman@gotham.com", "brucewayne", passwordEncryptionService.EncryptPassword(enteredPassword),
                "Ninja County", TimeZone.CurrentTimeZone, "", "");
            user.AutoLogout = new TimeSpan(0, 0, 0, 60);
            // Add this user to the MockUserRepository
            (userRepository as MockUserRepository).AddUser(user);

            bool exceptionRaised = false;
            try
            {
                loginApplicationService.Login(
                    new LoginCommand("brucewayne", "whereisthejoke"));
            }
            catch (InvalidCredentialException e)
            {
                exceptionRaised = true;
            }
            Assert.IsTrue(exceptionRaised);
        }
    }
}
