﻿using System;

namespace CoinExchange.Trades.Domain.Model.OrderAggregate
{
    /// <summary>
    /// serves the purpose for order side
    /// </summary>
    [Serializable]
    public enum OrderSide
    {
        Buy,
        Sell
    }
}
