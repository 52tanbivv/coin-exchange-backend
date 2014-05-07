﻿using System;
using System.Collections.Generic;
using CoinExchange.Trades.Application.MarketDataServices.Representation;
using CoinExchange.Trades.ReadModel.MemoryImages;

namespace CoinExchange.Trades.Application.MarketDataServices
{
    public interface IMarketDataQueryService
    {
        TickerRepresentation[] GetTickerInfo(string pairs);
        OhlcRepresentation GetOhlcInfo(string pair, int interval, string since);
        Tuple<OrderRepresentationList, OrderRepresentationList> GetOrderBook(string symbol, int count);
    }
}
