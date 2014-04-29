﻿/*
 * Author: Waqas
 * Comany: Aurora Solutions
 */

using System;
namespace CoinExchange.Trades.Domain.Model.OrderAggregate
{
    /// <summary>
    /// Represents the ID for an order. ValueObject
    /// </summary>
    [Serializable]
    public class OrderId
    {
        private readonly int _id;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="id"></param>
        public OrderId(int id)
        {
            _id = id;
        }

        /// <summary>
        /// The ID of the Order
        /// </summary>
        public int Id { get { return _id; } }

        public override bool Equals(object obj)
        {
            if (obj is OrderId)
            {
                return Id == (obj as OrderId).Id;
            }
            return false;
        }
    }
}
