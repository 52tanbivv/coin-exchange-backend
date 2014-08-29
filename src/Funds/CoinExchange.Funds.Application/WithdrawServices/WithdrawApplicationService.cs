﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Funds.Application.WithdrawServices.Commands;
using CoinExchange.Funds.Application.WithdrawServices.Representations;
using CoinExchange.Funds.Domain.Model.CurrencyAggregate;
using CoinExchange.Funds.Domain.Model.DepositAggregate;
using CoinExchange.Funds.Domain.Model.Repositories;
using CoinExchange.Funds.Domain.Model.Services;
using CoinExchange.Funds.Domain.Model.WithdrawAggregate;

namespace CoinExchange.Funds.Application.WithdrawServices
{
    /// <summary>
    /// Withdraw Application Service
    /// </summary>
    public class WithdrawApplicationService : IWithdrawApplicationService
    {
        private IFundsPersistenceRepository _fundsPersistenceRepository;
        private IWithdrawAddressRepository _withdrawAddressRepository;
        private ICoinClientService _coinClientService;
        private IFundsValidationService _fundsValidationService;
        private IWithdrawRepository _withdrawRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WithdrawApplicationService(IFundsPersistenceRepository fundsPersistenceRepository, 
            IWithdrawAddressRepository withdrawAddressRepository, ICoinClientService coinClientService, 
            IFundsValidationService fundsValidationService, IWithdrawRepository withdrawRepository)
        {
            _fundsPersistenceRepository = fundsPersistenceRepository;
            _withdrawAddressRepository = withdrawAddressRepository;
            _coinClientService = coinClientService;
            _fundsValidationService = fundsValidationService;
            _withdrawRepository = withdrawRepository;
        }

        /// <summary>
        /// Get recent withdrawals for hte given currency and account id
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public List<WithdrawRepresentation> GetRecentWithdrawals(int accountId, string currency)
        {
            List<WithdrawRepresentation> withdrawRepresentations = null;
            List<Withdraw> withdrawals = _withdrawRepository.GetWithdrawByCurrencyAndAccountId(currency, new AccountId(accountId));
            if (withdrawals != null && withdrawals.Any())
            {
                withdrawRepresentations = new List<WithdrawRepresentation>();
                foreach (var withdrawal in withdrawals)
                {
                    withdrawRepresentations.Add(new WithdrawRepresentation(withdrawal.Currency.Name, withdrawal.WithdrawId,
                        withdrawal.DateTime, withdrawal.Type.ToString(), withdrawal.Amount, withdrawal.Fee,
                        withdrawal.Status.ToString()));
                }
            }

            return withdrawRepresentations;
        }

        /// <summary>
        /// Add new Address
        /// </summary>
        /// <param name="addAddressCommand"></param>
        /// <returns></returns>
        public bool AddAddress(AddAddressCommand addAddressCommand)
        {
            // Create a new address and save in the database
            WithdrawAddress withdrawAddress = new WithdrawAddress(new Currency(addAddressCommand.Currency), 
                new BitcoinAddress(addAddressCommand.BitcoinAddress), addAddressCommand.Description,
                new AccountId(addAddressCommand.AccountId), DateTime.Now);

            _fundsPersistenceRepository.SaveOrUpdate(withdrawAddress);
            return true;
        }

        /// <summary>
        /// Gets the list of withdrawaal addresses
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public List<WithdrawAddressRepresentation> GetWithdrawalAddresses(int accountId, string currency)
        {
            // Get the list of withdraw addresses, extract the information into the withdraw address representation list and 
            // return 
            List<WithdrawAddressRepresentation> withdrawAddressRepresentations = new List<WithdrawAddressRepresentation>();
            List<WithdrawAddress> withdrawAddresses = _withdrawAddressRepository.GetWithdrawAddressByAccountId(new AccountId(accountId));
            foreach (var withdrawAddress in withdrawAddresses)
            {
                withdrawAddressRepresentations.Add(new WithdrawAddressRepresentation(withdrawAddress.BitcoinAddress.Value,
                    withdrawAddress.Description));
            }
            return withdrawAddressRepresentations;
        }

        /// <summary>
        /// Commit the Withdrawal from the user
        /// </summary>
        /// <param name="commitWithdrawCommand"></param>
        /// <returns></returns>
        public bool CommitWithdrawal(CommitWithdrawCommand commitWithdrawCommand)
        {
            // ToDo: Call fundsValidationService, confirm the withdrawal, get the withdrawal, send to BitcoinClient Service
            // and pass the withdrawal to Bitcoin Network
            // ToDo: Get TransactionID from the bitcoin Network. Confirm whether the transaction ID can be retreived
            // before making the withdrawal, or is assigned after the withdrawal is made
            Withdraw withdrawal = _fundsValidationService.ValidateFundsForWithdrawal(
                new AccountId(commitWithdrawCommand.AccountId), new Currency(commitWithdrawCommand.Currency), 
                commitWithdrawCommand.Amount, null/*Null until confirmation of what to use*/, 
                new BitcoinAddress(commitWithdrawCommand.BitcoinAddress));

            if (withdrawal != null)
            {
                // Commit the withdraw to the network, if successful, create transaction ledger using 
                // IFundsValidationService
                return _coinClientService.CommitWithdraw(withdrawal);
            }
            return false;
        }

        /// <summary>
        /// Get the threshold limits for the withdrawal
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public WithdrawLimitRepresentation GetWithdrawLimitThresholds(int accountId, string currency)
        {
            // Find the total limits and calculate the threshold limits used by the user
            AccountWithdrawLimits accountWithdrawLimits = _fundsValidationService.GetWithdrawThresholds(new AccountId(accountId), new Currency(currency));
            if (accountWithdrawLimits != null)
            {
                // Convert to the application layer presentation and return
                return new WithdrawLimitRepresentation(accountWithdrawLimits.Currency.Name, 
                    accountWithdrawLimits.AccountId.Value, accountWithdrawLimits.DailyLimit,
                    accountWithdrawLimits.DailyLimitUsed, accountWithdrawLimits.MonthlyLimit, 
                    accountWithdrawLimits.MonthlyLimitUsed, accountWithdrawLimits.CurrentBalance, 
                    accountWithdrawLimits.MaximumWithdraw, accountWithdrawLimits.MaximumWithdrawInUsd,
                    accountWithdrawLimits.Withheld, accountWithdrawLimits.WithheldInUsd, accountWithdrawLimits.Fee);
            }
            return null;
        }
    }
}
