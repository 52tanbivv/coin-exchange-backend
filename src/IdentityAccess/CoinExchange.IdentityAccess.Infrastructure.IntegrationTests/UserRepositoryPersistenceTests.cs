﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.IdentityAccess.Domain.Model.Repositories;
using CoinExchange.IdentityAccess.Domain.Model.UserAggregate;
using NHibernate;
using NHibernate.Mapping;
using NUnit.Framework;
using Spring.Data.NHibernate;

namespace CoinExchange.IdentityAccess.Infrastructure.IntegrationTests
{
    [TestFixture]
    public class UserRepositoryPersistenceTests:AbstractConfiguration
    {
        private IPersistRepository _persistenceRepository;
        private IUserRepository _userRepository;
        private ISessionFactory _sessionFactory;

        //properties will be injected based on type
        public IUserRepository UserRepository
        {
            set { _userRepository = value; }
        }
        public IPersistRepository PersistenceRepository
        {
            set { _persistenceRepository = value; }
        }

        [Test]
        [Category("Integration")]
        public void CreateNewUser_PersistUserAndGetThatUserFromDB_BothUserAreSame()
        {
           User user=new User("NewUser","asdf","12345","xyz","user88@gmail.com",Language.English, TimeZone.CurrentTimeZone,new TimeSpan(1,1,1,1),DateTime.Now,"Pakistan","","2233344","1234"); 
           _persistenceRepository.SaveUpdate(user);
           User receivedUser = _userRepository.GetUserByUserName("NewUser");
            Assert.NotNull(receivedUser);
            Assert.AreEqual(user.Username,receivedUser.Username);
            Assert.AreEqual(user.Password, receivedUser.Password);
            Assert.AreEqual(user.PublicKey, receivedUser.PublicKey);
            Assert.AreEqual(user.Language, receivedUser.Language);
            Assert.AreEqual(user.AutoLogout, receivedUser.AutoLogout);
            Assert.AreEqual(user.TimeZone.ToString(),receivedUser.TimeZone.ToString());
            Assert.AreEqual(user.Country, receivedUser.Country);
            Assert.AreEqual(user.State, receivedUser.State);
            Assert.AreEqual(user.PhoneNumber, receivedUser.PhoneNumber);
            Assert.AreEqual(user.Address1,receivedUser.Address1);
            Assert.AreEqual(user.ActivationKey, receivedUser.ActivationKey);
        }

        [Test]
        [Category("Integration")]
        public void CreateNewUser_PersistUserAndGetThatUserByEmailFromDatabase_BothUserAreSame()
        {
            User user = new User("NewUser", "asdf", "12345", "xyz", "user88@gmail.com", Language.English, TimeZone.CurrentTimeZone, new TimeSpan(1, 1, 1, 1), DateTime.Now, "Pakistan", "", "2233344", "1234");
            _persistenceRepository.SaveUpdate(user);
            User receivedUser = _userRepository.GetUserByEmail("User88@Gmail.com");
            Assert.NotNull(receivedUser);
            Assert.AreEqual(user.Username, receivedUser.Username);
            Assert.AreEqual(user.Password, receivedUser.Password);
            Assert.AreEqual(user.PublicKey, receivedUser.PublicKey);
            Assert.AreEqual(user.Language, receivedUser.Language);
            Assert.AreEqual(user.AutoLogout, receivedUser.AutoLogout);
            Assert.AreEqual(user.TimeZone.ToString(), receivedUser.TimeZone.ToString());
            Assert.AreEqual(user.Country, receivedUser.Country);
            Assert.AreEqual(user.State, receivedUser.State);
            Assert.AreEqual(user.PhoneNumber, receivedUser.PhoneNumber);
            Assert.AreEqual(user.Address1, receivedUser.Address1);
            Assert.AreEqual(user.ActivationKey, receivedUser.ActivationKey);
        }

        [Test]
        [Category("Integration")]
        public void CreateNewUser_PersistUserAndGetThatUserByActivationKeyFromDatabase_BothUserAreSame()
        {
            User user = new User("NewUser", "asdf", "12345", "xyz", "user88@gmail.com", Language.English, TimeZone.CurrentTimeZone, new TimeSpan(1, 1, 1, 1), DateTime.Now, "Pakistan", "", "2233344", "1234");
            _persistenceRepository.SaveUpdate(user);
            User receivedUser = _userRepository.GetUserByActivationKey("1234");
            Assert.NotNull(receivedUser);
            Assert.AreEqual(user.Username, receivedUser.Username);
            Assert.AreEqual(user.Password, receivedUser.Password);
            Assert.AreEqual(user.PublicKey, receivedUser.PublicKey);
            Assert.AreEqual(user.Language, receivedUser.Language);
            Assert.AreEqual(user.AutoLogout, receivedUser.AutoLogout);
            Assert.AreEqual(user.TimeZone.ToString(), receivedUser.TimeZone.ToString());
            Assert.AreEqual(user.Country, receivedUser.Country);
            Assert.AreEqual(user.State, receivedUser.State);
            Assert.AreEqual(user.PhoneNumber, receivedUser.PhoneNumber);
            Assert.AreEqual(user.Address1, receivedUser.Address1);
            Assert.AreEqual(user.ActivationKey, receivedUser.ActivationKey);
        }

        [Test]
        [Category("Integration")]
        public void CreateNewUser_PersistUserAndGetThatUserByUsernameAndEmailFromDatabase_BothUserAreSame()
        {
            User user = new User("NewUser", "asdf", "12345", "xyz", "user88@gmail.com", Language.English, TimeZone.CurrentTimeZone, new TimeSpan(1, 1, 1, 1), DateTime.Now, "Pakistan", "", "2233344", "1234");
            _persistenceRepository.SaveUpdate(user);
            User receivedUser = _userRepository.GetUserByEmailAndUserName("NewUser", "user88@gmail.com");
            Assert.NotNull(receivedUser);
            Assert.AreEqual(user.Username, receivedUser.Username);
            Assert.AreEqual(user.Password, receivedUser.Password);
            Assert.AreEqual(user.PublicKey, receivedUser.PublicKey);
            Assert.AreEqual(user.Language, receivedUser.Language);
            Assert.AreEqual(user.AutoLogout, receivedUser.AutoLogout);
            Assert.AreEqual(user.TimeZone.ToString(), receivedUser.TimeZone.ToString());
            Assert.AreEqual(user.Country, receivedUser.Country);
            Assert.AreEqual(user.State, receivedUser.State);
            Assert.AreEqual(user.PhoneNumber, receivedUser.PhoneNumber);
            Assert.AreEqual(user.Address1, receivedUser.Address1);
            Assert.AreEqual(user.ActivationKey, receivedUser.ActivationKey);
        }

    }
}
