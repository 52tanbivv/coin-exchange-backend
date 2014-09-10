﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoinExchange.IdentityAccess.Domain.Model.UserAggregate;

namespace CoinExchange.IdentityAccess.Application.UserServices.Representations
{
    /// <summary>
    /// Response for hte Account Settings for a user
    /// </summary>
    public class AccountSettingsRepresentation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountSettingsRepresentation"/> class.
        /// </summary>
        public AccountSettingsRepresentation(string username, string email, string pgpPublicKey, Language language, TimeZone timeZone, bool isDefaultAutoLogout, int autoLogoutMinutes)
        {
            Username = username;
            Email = email;
            PgpPublicKey = pgpPublicKey;
            Language = language;
            TimeZone = timeZone;
            IsDefaultAutoLogout = isDefaultAutoLogout;
            AutoLogoutMinutes = autoLogoutMinutes;

            LanguagesAvailable = Enum.GetNames(typeof (Language));
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// PGP Public Key
        /// </summary>
        public string PgpPublicKey { get; private set; }

        /// <summary>
        /// Language
        /// </summary>
        public Language Language { get; private set; }

        /// <summary>
        /// TimeZone
        /// </summary>
        public TimeZone TimeZone { get; private set; }

        /// <summary>
        /// Specifies if the Auto logout time is the Custom(specified by the user betwenn 2 and 240 minutes) or Default
        /// </summary>
        public bool IsDefaultAutoLogout { get; private set; }

        /// <summary>
        /// The minutes after which the User will logout automatically
        /// </summary>
        public int AutoLogoutMinutes { get; private set; }

        /// <summary>
        /// The list of languages availabel to offer
        /// </summary>
        public string[] LanguagesAvailable { get; private set; }
    }
}
