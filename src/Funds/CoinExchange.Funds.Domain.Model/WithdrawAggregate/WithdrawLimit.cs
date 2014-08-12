﻿
namespace CoinExchange.Funds.Domain.Model.WithdrawAggregate
{
    /// <summary>
    /// Represents the daily and monthly limits for withdrawal
    /// </summary>
    public class WithdrawLimit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WithdrawLimit()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WithdrawLimit(string tierLevel, decimal dailyLimit, decimal monthlyLimit)
        {
            TierLevel = tierLevel;
            DailyLimit = dailyLimit;
            MonthlyLimit = monthlyLimit;            
        }

        /// <summary>
        /// Database Primary key
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Tier Level
        /// </summary>
        public string TierLevel { get; private set; }

        /// <summary>
        /// Monthly Limit
        /// </summary>
        public decimal MonthlyLimit { get; private set; }

        /// <summary>
        /// Daily Limit
        /// </summary>
        public decimal DailyLimit { get; private set; }
    }
}
