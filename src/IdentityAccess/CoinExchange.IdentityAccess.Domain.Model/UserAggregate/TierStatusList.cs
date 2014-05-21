﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinExchange.IdentityAccess.Domain.Model.UserAggregate
{
    /// <summary>
    /// Represents the list of the Tiers for a user
    /// </summary>
    public class TierStatusList : IEnumerable<UserTierStatus>
    {
        private List<UserTierStatus> _tierStatusList = new List<UserTierStatus>();

        /// <summary>
        /// Add an element
        /// </summary>
        /// <returns></returns>
        internal bool AddTierStatus(UserTierStatus userTierStatus)
        {
            _tierStatusList.Add(userTierStatus);
            return true;
        }

        /// <summary>
        /// Remove an element
        /// </summary>
        /// <param name="userTierStatus"></param>
        /// <returns></returns>
        internal bool RemoveTierStatus(UserTierStatus userTierStatus)
        {
            _tierStatusList.Remove(userTierStatus);
            return true;
        }

        /// <summary>
        /// GetEnumerator - Specific
        /// </summary>
        /// <returns></returns>
        public IEnumerator<UserTierStatus> GetEnumerator()
        {
            foreach (var tiaerStatus in _tierStatusList)
            {
                // Lets check for end of list (its bad code since we used arrays)
                if (tiaerStatus == null)
                {
                    break;
                }

                // Return the current element and then on next function call 
                // resume from next element rather than starting all over again;
                yield return tiaerStatus;
            }
        }

        /// <summary>
        /// GetEnumerator - Generic
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
