﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinExchange.Funds.Application.DepositServices.Representations
{
    /// <summary>
    /// Representation for a deposit ledger
    /// </summary>
    public class DepositRepresentation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DepositRepresentation(string currency, string description, string depositId, DateTime date, decimal amount, string status,
            string bitcoinAddress, string transactionId)
        {
            Currency = currency;
            Description = description;
            DepositId = depositId;
            Date = date;
            Amount = amount;
            Status = status;
            BitcoinAddress = bitcoinAddress;
            TransactionId = transactionId;
        }

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; private set; }

        /// <summary>
        /// Currency
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// DepositId
        /// </summary>
        public string DepositId { get; private set; }

        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Amount
        /// </summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// Status of the Deposit
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Bitcoin Address
        /// </summary>
        public string BitcoinAddress { get; private set; }

        /// <summary>
        /// Transaction ID
        /// </summary>
        public string TransactionId { get; private set; }
    }
}
