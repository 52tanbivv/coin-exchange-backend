﻿using System;
using System.Collections.Generic;
using CoinExchange.Trades.Domain.Model.OrderAggregate;
using CoinExchange.Trades.Domain.Model.TradeAggregate;
using CoinExchange.Trades.ReadModel.DTO;

namespace CoinExchange.Trades.ReadModel.Repositories
{
    /// <summary>
    /// Order Repository
    /// </summary>
    public interface IOrderRepository
    {
        List<OrderReadModel> GetOpenOrders(string traderId);
        List<OrderReadModel> GetClosedOrders(string traderId, DateTime start, DateTime end);
        List<OrderReadModel> GetClosedOrders(string traderId);
        List<OrderReadModel> GetAllOrderOfTrader(string traderId);
        OrderReadModel GetOrderById(string orderId);
        OrderReadModel GetOrderById(TraderId traderId,OrderId orderId);
        void RollBack();
    }
}
