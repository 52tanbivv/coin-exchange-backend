﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinExchange.Funds.Domain.Model.DepositAggregate
{
    /// <summary>
    /// Interface for Deposit Address Repository
    /// </summary>
    public interface IDepositAddressRepository
    {
        DepositAddress GetDepositAddressById(int id);
        List<DepositAddress> GetDepositAddressByAccountId(AccountId accountId);
        List<DepositAddress> GetDepositAddressByAccountIdAndCurrency(AccountId accountId, string currency);
        DepositAddress GetDepositAddressByAddress(BitcoinAddress bitcoinAddress);
        List<DepositAddress> GetAllDepositAddresses();
    }
}
