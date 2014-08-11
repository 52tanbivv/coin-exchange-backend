﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Funds.Domain.Model.LedgerAggregate;

namespace CoinExchange.Funds.Domain.Model.WithdrawAggregate
{
    /// <summary>
    /// Service to determine the maximum Withdrawal amount
    /// </summary>
    public class WithdrawLimitEvaluationService : IWithdrawLimitEvaluationService
    {
        private double _dailyLimit = 0;
        private double _dailyLimitUsed = 0;
        private double _monthlyLimit = 0;
        private double _monthlyLimitUsed = 0;
        private double _maximumWithdraw = 0;
        private double _maximumWithdrawUsd = 0;
        private double _withheld = 0;
        private double _withheldConverted = 0;

        /// <summary>
        /// Evaluate if the user is eligible to withdraw the desired amount or not
        /// </summary>
        /// <param name="withdrawAmountUsd"></param>
        /// <param name="withdrawLedgers"></param>
        /// <param name="withdrawLimit"></param>
        /// <param name="bestBidPrice"></param>
        /// <param name="bestAskPrice"></param>
        /// <param name="availableBalance"></param>
        /// <param name="currentBalance"></param>
        /// <returns></returns>
        public bool EvaluateMaximumWithdrawLimit(double withdrawAmountUsd, IList<Ledger> withdrawLedgers,
            WithdrawLimit withdrawLimit, double bestBidPrice, double bestAskPrice, double availableBalance, 
            double currentBalance)
        {
            // Set Daily and Monthly Limit
            SetLimits(withdrawLimit);
            // Set the amount used in the Daily and Monthly limit
            SetUsedLimitsUsd(withdrawLedgers);
            // Evaluate the Maximum Withdraw, set it, and return response whether it went successfully or not
            if (EvaluateMaximumWithdrawUsd(bestBidPrice, bestAskPrice))
            {
                // If we do not have sufficient balance, then the maximum withdrawal amount is the balance that we have 
                // at our disposal
                if (availableBalance < _maximumWithdraw)
                {
                    _maximumWithdraw = Math.Round(availableBalance, 5);
                    _maximumWithdrawUsd = Math.Round(ConvertCurrencyToUsd(bestBidPrice, bestAskPrice, availableBalance), 5);                   
                }
                _withheld = Math.Round(currentBalance - availableBalance, 5);
                _withheldConverted = Math.Round(ConvertCurrencyToUsd(bestBidPrice, bestAskPrice,
                    currentBalance - availableBalance), 5);

                // If the current withdraw amount is less than the maximum withdraw
                return withdrawAmountUsd <= _maximumWithdrawUsd;
            }
            return false;
        }

        /// <summary>
        /// Evaluates the value of Maximum Withdraw
        /// </summary>
        /// <returns></returns>
        private bool EvaluateMaximumWithdrawUsd(double bestBid, double bestAsk)
        {
            // If either the daily limit or monthly limit has been reached, no wihtdraw can be made
            if (!(_monthlyLimit - _monthlyLimitUsed).Equals(0) || !(_dailyLimit - _dailyLimitUsed).Equals(0))
            {
                // If both the used limits have not been used
                if (_monthlyLimitUsed.Equals(0) && _dailyLimitUsed.Equals(0))
                {
                    return SetMaximumWithdraw(_dailyLimit, bestBid, bestAsk);
                }
                // If only the DailyLimitUsed is 0, we need to evaluate other conditions
                else if (_dailyLimitUsed.Equals(0))
                {
                    // E.g., if DailyLimit = 0/1000 & MonthlyLimit = 3000/5000
                    if (_monthlyLimit - _monthlyLimitUsed > _dailyLimit)
                    {
                        return SetMaximumWithdraw(_dailyLimit, bestBid, bestAsk);
                    }
                    // E.g., if DailyLimit = 0/1000, & MonthlyLimit = 4500/5000
                    else if (_monthlyLimit - _monthlyLimitUsed < _dailyLimit)
                    {
                        return SetMaximumWithdraw(_monthlyLimit - _monthlyLimitUsed, bestBid, bestAsk);
                    }
                }
                // E.g., if DailyLimit = 500/1000 & MonthlyLimit = 4000/5000
                else if ((_monthlyLimit - _monthlyLimitUsed) > (_dailyLimit - _dailyLimitUsed))
                {
                    return SetMaximumWithdraw(_dailyLimit - _dailyLimitUsed, bestBid, bestAsk);
                }
                // E.g., if DailyLimit = 400/1000 & MonthlyLimit = 4500/5000
                else if ((_monthlyLimit - _monthlyLimitUsed) < (_dailyLimit - _dailyLimitUsed))
                {
                    return SetMaximumWithdraw(_monthlyLimit - _monthlyLimitUsed, bestBid, bestAsk);
                }
                else if ((_monthlyLimit - _monthlyLimitUsed).Equals(_dailyLimit - _dailyLimitUsed))
                {
                    return SetMaximumWithdraw(_monthlyLimit - _monthlyLimitUsed, bestBid, bestAsk);
                }
            }
            return false;
        }

        /// <summary>
        /// Set the value for the Maximum withdraw in currency amount and US Dollars
        /// </summary>
        private bool SetMaximumWithdraw(double maximumWithdraw, double bestBid, double bestAsk)
        {
            _maximumWithdraw = ConvertUsdToCurrency(bestBid, bestAsk, maximumWithdraw);
            _maximumWithdrawUsd = maximumWithdraw;
            return true;
        }

        /// <summary>
        /// Converts US Dollars to currency amount
        /// </summary>
        private double ConvertUsdToCurrency(double bestBid, double bestAsk, double usdAmount)
        {
            double sum = (usdAmount / bestBid) + (usdAmount / bestAsk);
            double midPoint = sum / 2;
            return midPoint;
        }

        /// <summary>
        /// Converts US Dollars to currency amount
        /// </summary>
        private double ConvertCurrencyToUsd(double bestBid, double bestAsk, double currencyAmount)
        {
            double sum = (currencyAmount * bestBid) + (currencyAmount * bestAsk);
            double midPoint = sum / 2;
            return midPoint;
        }

        /// <summary>
        /// Sets teh limits of daily and monthly Withdraw limits
        /// </summary>
        private void SetLimits(WithdrawLimit withdrawLimit)
        {
            _dailyLimit = withdrawLimit.DailyLimit;
            _monthlyLimit = withdrawLimit.MonthlyLimit;
        }

        /// <summary>
        /// Sets the amount that has been used for daily and monthly Withdraw limits
        /// </summary>
        /// <returns></returns>
        private void SetUsedLimitsUsd(IList<Ledger> withdrawLedgers)
        {
            double tempDailyLimitUsed = 0;
            double tempMonthlyLimitUsed = 0;
            foreach (var withdrawLedger in withdrawLedgers)
            {
                if (withdrawLedger.DateTime >= DateTime.Now.AddHours(-24))
                {
                    tempDailyLimitUsed += withdrawLedger.AmountInUsd;
                    tempMonthlyLimitUsed += withdrawLedger.AmountInUsd;
                }
                if (withdrawLedger.DateTime >= DateTime.Now.AddDays(-30) && withdrawLedger.DateTime < DateTime.Now.AddHours(-24))
                {
                    tempMonthlyLimitUsed += withdrawLedger.AmountInUsd;
                }
            }

            _dailyLimitUsed = tempDailyLimitUsed;
            _monthlyLimitUsed = tempMonthlyLimitUsed;
        }


        /// <summary>
        /// Daily Limit
        /// </summary>
        public double DailyLimit { get { return _dailyLimit; } private set { _dailyLimit = value; } }

        /// <summary>
        /// Daily Limit Used
        /// </summary>
        public double DailyLimitUsed { get { return _dailyLimitUsed; } private set { _dailyLimitUsed = value; } }

        /// <summary>
        /// Monthly Limit
        /// </summary>
        public double MonthlyLimit { get { return _monthlyLimit; } private set { _monthlyLimit = value; } }

        /// <summary>
        /// Monthly limit used
        /// </summary>
        public double MonthlyLimitUsed { get { return _monthlyLimitUsed; } private set { _monthlyLimitUsed = value; } }

        /// <summary>
        /// Withheld Amount
        /// </summary>
        public double WithheldAmount { get { return _withheld; } private set { _withheld = value; } }

        public double WithheldConverted { get { return _withheldConverted; } private set { _withheldConverted = value; } }

        /// <summary>
        /// Maximum withdraw amount
        /// </summary>
        public double MaximumWithdraw { get { return _maximumWithdraw; } private set { _maximumWithdraw = value; } }

        /// <summary>
        /// Maximum Withdrawal amount in US Dollars
        /// </summary>
        public double MaximumWithdrawUsd { get { return _maximumWithdrawUsd; } private set { _maximumWithdrawUsd = value; } }
    }
}
