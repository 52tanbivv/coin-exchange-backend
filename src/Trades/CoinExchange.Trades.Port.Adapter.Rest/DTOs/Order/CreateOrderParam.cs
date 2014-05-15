﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinExchange.Trades.Port.Adapter.Rest.DTOs.Order
{
    /// <summary>
    /// Order parameters to be received from user
    /// </summary>
    public class CreateOrderParam
    {
        public string Pair;
        public string Side;
        public string Type;
        public decimal Price;
        public decimal Volume;

        /// <summary>
        /// Custom To string method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Order parameters, Currency Pair={0},Side={1},Type={2},Price={3}," +
                                 "Volume={4}", Pair, Side, Type, Price, Volume);
        }
    }
}
