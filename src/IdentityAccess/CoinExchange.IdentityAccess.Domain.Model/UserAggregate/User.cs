﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace CoinExchange.IdentityAccess.Domain.Model.UserAggregate
{
    /// <summary>
    /// Specifies the user and their related information after the ysign up for CoinExchange
    /// </summary>
    public class User
    {
        private string _username;
        private string _password;
        private string _pgpPublicKey;
        private string _address1;
        private string _address2;
        private string _email;
        private Language _language;
        private TimeZone _timeZone;
        private TimeSpan _autoLogout;
        private DateTime _lastLogin;
        private TierStatusList _tierStatusList;
        private UserDocumentsList _userDocumentsList;
        private string _phoneNumber;
        private string _country;
        private string _state;
        private string _activationKey;
        private List<PasswordCodeRecord> _forgottenPasswordCodesList = new List<PasswordCodeRecord>();

        //default constructor
        public User()
        {
            
        }

        /// <summary>
        /// Constructor for Sign Up
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="country"></param>
        /// <param name="timeZone"></param>
        /// <param name="pgpPublicKey"></param>
        /// <param name="activationKey"></param>
        public User(string email, string username, string password, string country, TimeZone timeZone,
            string pgpPublicKey, string activationKey)
        {
            _username = username;
            _password = password;
            _pgpPublicKey = pgpPublicKey;
            _country = country;
            _email = email;
            _timeZone = timeZone;
            _activationKey = activationKey;
            _autoLogout = new TimeSpan(0,0,10,0);

            _tierStatusList = new TierStatusList();
            _userDocumentsList = new UserDocumentsList();
        }

        /// <summary>
        /// Constructor for filling in all the information
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="pgpPublicKey"></param>
        /// <param name="address"></param>
        /// <param name="email"></param>
        /// <param name="language"></param>
        /// <param name="timeZone"></param>
        /// <param name="autoLogout"></param>
        /// <param name="lastLogin"></param>
        /// <param name="country"> </param>
        /// <param name="state"> </param>
        /// <param name="phoneNumber"> </param>
        /// <param name="activationKey"> </param>
        public User(string username, string password, string pgpPublicKey, string address, string email, Language language,
            TimeZone timeZone, TimeSpan autoLogout, DateTime lastLogin, string country, string state, string phoneNumber, 
            string activationKey)
        {
             Username = username;
            _password = password;
            _pgpPublicKey = pgpPublicKey;
            Address1 = address;
            _email = email;
            _language = language;
            _timeZone = timeZone;
            _autoLogout = autoLogout;
            _lastLogin = lastLogin;
            Country = country;
            PhoneNumber = phoneNumber;
            State = state;
            ActivationKey = activationKey;

            _tierStatusList = new TierStatusList();
            _userDocumentsList = new UserDocumentsList();
        }

        #region Methods

        /// <summary>
        /// Add User Tier Status
        /// </summary>
        /// <param name="userTierStatus"></param>
        /// <returns></returns>
        public bool AddTierStatus(UserTierStatus userTierStatus)
        {
            _tierStatusList.AddTierStatus(userTierStatus);
            return true;
        }

        /// <summary>
        /// Remove a Tier status for a user
        /// </summary>
        /// <param name="userTierStatus"></param>
        /// <returns></returns>
        public bool RemoveTierStatus(UserTierStatus userTierStatus)
        {
            _tierStatusList.RemoveTierStatus(userTierStatus);
            return true;
        }

        /// <summary>
        /// Adds a new Forgot Password code to this user
        /// </summary>
        /// <param name="forgotPasswordCode"></param>
        /// <returns></returns>
        public bool AddForgotPasswordCode(string forgotPasswordCode)
        {
            if (this.ForgotPasswordCode != null && this.ForgotPasswordCodeExpiration > DateTime.Now)
            {
                // Assign the latest code
                this.ForgotPasswordCode = forgotPasswordCode;
                this.ForgotPasswordCodeExpiration = DateTime.Now.AddHours(2);
                // Add this code to the list of the Forgotten Password Codes till date for this user
                _forgottenPasswordCodesList.Add(new PasswordCodeRecord(forgotPasswordCode,
                                                                       Convert.ToDateTime(
                                                                           this.ForgotPasswordCodeExpiration),
                                                                       DateTime.Now));
                return true;
            }
            throw new InvalidOperationException("Last Forgot PasswordCode request hasn't expired yet.");
        }

        /// <summary>
        /// Checks if this user's latest ForgotPasswordCode's validity period still remaining. Returns true if yes
        /// </summary>
        /// <returns></returns>
        public bool IsPasswordCodeValid()
        {
            if (!string.IsNullOrEmpty(this.ForgotPasswordCode))
            {
                    if (this.ForgotPasswordCodeExpiration > DateTime.Now)
                    {
                        // No active ForgotPassword code at the moment
                        this.ForgotPasswordCode = null;
                        this.ForgotPasswordCodeExpiration = null;

                        // Iterate to see which Password code has been used
                        foreach (var passwordCodeRecord in _forgottenPasswordCodesList)
                        {
                            if (passwordCodeRecord.ExpirationDateTime.Equals(this.ForgotPasswordCodeExpiration))
                            {
                                // Mark that this Password Code has been used to reset the password
                                passwordCodeRecord.MarkCodeUsage();
                                break;
                            }
                        }
                        return true;
                    }
                    // No active ForgotPassword code at the moment
                    this.ForgotPasswordCode = null;
                    this.ForgotPasswordCodeExpiration = null;
                throw new TimeoutException("Timeout expired for resetting the password.");
            }
            throw new NullReferenceException("Password Code is null or does not contain any value");
        }

        #endregion Methods

        /// <summary>
        /// Username (Once set cannot be changed)
        /// </summary>
        public string Username { get { return _username; } private set { _username = value; } }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get { return _password; } set { _password = value; } }

        /// <summary>
        /// Public Key
        /// </summary>
        public string PublicKey { get { return _pgpPublicKey; } set { _pgpPublicKey = value; } }

        /// <summary>
        /// Address 1
        /// </summary>
        public string Address1 { get { return _address1; } set { _address1 = value; } }

        /// <summary>
        /// Address 2
        /// </summary>
        public string Address2 { get { return _address2; } set { _address2 = value; } }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get { return _email; } set { _email = value; } }

        /// <summary>
        /// Language
        /// </summary>
        public Language Language { get { return _language; } set { _language = value; } }

        /// <summary>
        /// TimeZone
        /// </summary>
        public TimeZone TimeZone { get { return _timeZone; } set { _timeZone = value; } }

        /// <summary>
        /// AutoLogout
        /// </summary>
        public TimeSpan AutoLogout { get { return _autoLogout; } set { _autoLogout = value; } }

        /// <summary>
        /// Last Login
        /// </summary>
        public DateTime LastLogin { get { return _lastLogin; } set { _lastLogin = value; } }

        /// <summary>
        /// Country State of user
        /// </summary>
        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// Residence Country
        /// </summary>
        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }

        /// <summary>
        /// Phone Number
        /// </summary>
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; }
        }

        /// <summary>
        /// Activation Key
        /// </summary>
        public string ActivationKey
        {
            get { return _activationKey; }
            set
            {
                _activationKey = value;
            }
        }

        /// <summary>
        /// Has the Activation Key already been used to activate this account
        /// </summary>
        public IsActivationKeyUsed IsActivationKeyUsed { get; set; }

        /// <summary>
        /// Is this User blocked
        /// </summary>
        public IsUserBlocked IsUserBlocked { get; set; }

        /// <summary>
        /// List of the Tier associated with this user
        /// </summary>
        public TierStatusList TierStatusList { get { return _tierStatusList; } private set { _tierStatusList = value; } }

        /// <summary>
        /// List of Ueser Docuemnts associated with this User
        /// </summary>
        public UserDocumentsList UserDocumentsList { get; set; }

        /// <summary>
        /// Is this user deleted softly from the database
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Code generated in case user has forgotten their password
        /// </summary>
        public string ForgotPasswordCode { get; set; }

        /// <summary>
        /// Represents an array of the forgotten password codes for this user
        /// </summary>
        public PasswordCodeRecord[] ForgottenPasswordCodes { get { return _forgottenPasswordCodesList.ToArray(); } }

        /// <summary>
        /// Validity Period of the latest ForgotPasswordCode
        /// </summary>
        public DateTime? ForgotPasswordCodeExpiration { get; private set; }
    }
}