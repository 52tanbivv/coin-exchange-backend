﻿using System;
using CoinExchange.Funds.Domain.Model.DepositAggregate;

namespace CoinExchange.Funds.Domain.Model.Services
{
    /// <summary>
    /// Interface for Funds validation
    /// </summary>
    public interface IFundsValidationService
    {
        /// <summary>
        /// Validates the Funds before sending an order
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="baseCurrency"></param>
        /// <param name="quoteCurrency"> </param>
        /// <param name="volume"></param>
        /// <param name="price"> </param>
        /// <param name="orderSide"></param>
        /// <returns></returns>
        bool ValidateFundsForOrder(AccountId accountId, CurrencyAggregate.Currency baseCurrency, 
            CurrencyAggregate.Currency quoteCurrency, double volume, double price, string orderSide);

        /// <summary>
        /// Validates that enough funds exist for the requested withdrawal to be made
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        bool ValidateFundsForWithdrawal(AccountId accountId, CurrencyAggregate.Currency currency, double amount);

        /// <summary>
        /// Handles the event that withdraw has been confirmed and takes the necessary steps
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="transactionId"></param>
        /// <param name="bitcoinAddress"></param>
        /// <returns></returns>
        bool WithdrawalExecuted(AccountId accountId, CurrencyAggregate.Currency currency, double amount, TransactionId
            transactionId, BitcoinAddress bitcoinAddress);

        /// <summary>
        /// Handles the event that a Deposit has been made and performes the necesssary options
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="transactionId"></param>
        /// <param name="bitcoinAddress"></param>
        /// <returns></returns>
        bool DepositConfirmed(AccountId accountId, CurrencyAggregate.Currency currency, double amount, TransactionId
            transactionId, BitcoinAddress bitcoinAddress);

        /// <summary>
        /// Handles the event that a trade has been executed and performs the necessay steps to update the balance and 
        /// create a transaction record
        /// </summary>
        /// <param name="currencyPair"></param>
        /// <param name="tradeVolume"></param>
        /// <param name="price"></param>
        /// <param name="executionDateTime"></param>
        /// <param name="tradeId"></param>
        /// <param name="buyAccountId"></param>
        /// <param name="sellAccountId"></param>
        /// <param name="buyOrderId"></param>
        /// <param name="sellOrderId"></param>
        /// <returns></returns>
        bool TradeExecuted(string currencyPair, double tradeVolume, double price,
            DateTime executionDateTime, string tradeId, string buyAccountId, string sellAccountId, string buyOrderId,
            string sellOrderId);
    }
}
