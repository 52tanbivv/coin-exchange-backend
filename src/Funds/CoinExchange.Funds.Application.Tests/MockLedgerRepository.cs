﻿using System;
using System.Collections.Generic;
using CoinExchange.Funds.Domain.Model.DepositAggregate;
using CoinExchange.Funds.Domain.Model.LedgerAggregate;

namespace CoinExchange.Funds.Application.Tests
{
    public class MockLedgerRepository : ILedgerRepository
    {
        private IList<Ledger> _ledgers = new List<Ledger>(); 
        public Ledger GetLedgerById(int id)
        {
            throw new NotImplementedException();
        }

        public List<Ledger> GetLedgerByAccountId(AccountId accountId)
        {
            throw new NotImplementedException();
        }

        public List<Ledger> GetLedgerByCurrencyName(string currency)
        {
            throw new NotImplementedException();
        }

        public IList<Ledger> GetLedgerByAccountIdAndCurrency(string currency, AccountId accountId)
        {
            IList<Ledger> currentLedgers = new List<Ledger>();
            foreach (var ledger in _ledgers)
            {
                if (ledger.Currency.Name == currency && ledger.AccountId.Value == accountId.Value)
                {
                    currentLedgers.Add(ledger);
                }
            }
            return currentLedgers;
        }

        public Ledger GetLedgerByLedgerId(string ledgerId)
        {
            throw new NotImplementedException();
        }

        public List<Ledger> GetLedgersByTradeId(string tradeId)
        {
            throw new NotImplementedException();
        }

        public Ledger GetLedgersByDepositId(string depositId)
        {
            throw new NotImplementedException();
        }

        public Ledger GetLedgersByWithdrawId(string withdrawId)
        {
            throw new NotImplementedException();
        }

        public List<Ledger> GetLedgersByOrderId(string orderId)
        {
            throw new NotImplementedException();
        }

        public decimal GetBalanceForCurrency(string currency, AccountId accountId)
        {
            throw new NotImplementedException();
        }

        public IList<Ledger> GetAllLedgers(int accountId)
        {
            throw new NotImplementedException();
        }

        public IList<Ledger> GetAllLedgers()
        {
            throw new NotImplementedException();
        }

        public void AddLedger(Ledger ledger)
        {
            _ledgers.Add(ledger);
        }
    }
}
