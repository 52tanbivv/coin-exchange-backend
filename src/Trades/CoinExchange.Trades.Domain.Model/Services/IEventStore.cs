﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Trades.Domain.Model.TradeAggregate;

namespace CoinExchange.Trades.Domain.Model.Services
{
    public interface IEventStore
    {
        bool StoreEvent(object blob);
        object GetEvent(string id);
        IList<Trade> GetTradeEventsFromOrderId(string id);
    }
}
