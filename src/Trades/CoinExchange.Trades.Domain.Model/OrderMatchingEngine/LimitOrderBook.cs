﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoinExchange.Common.Domain.Model;
using CoinExchange.Trades.Domain.Model.OrderAggregate;
using CoinExchange.Trades.Domain.Model.TradeAggregate;

namespace CoinExchange.Trades.Domain.Model.OrderMatchingEngine
{
    /// <summary>
    /// Book containing the limit orders for a particular currency pair
    /// </summary>
    [Serializable]
    public class LimitOrderBook
    {
        // Get the Current Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _currencyPair = string.Empty;
        private int _trasactionId = 0;

        /// <summary>
        /// Bid list
        /// </summary>
        private OrderList _bids = null;

        /// <summary>
        /// Ask list
        /// </summary>
        private OrderList _asks = null;

        // Listeners
        private TradeListener _tradeListener = null;
        private OrderListener _orderListener = null;
        private OrderBookListener _orderBookListener = null;

        // Events
        public event Action<Trade> TradeExecuted;
        public event Action<Order> OrderChanged;
        public event Action<LimitOrderBook> OrderBookChanged;
        public event Action<Order, Price, Volume> OrderAccepted;
        public event Action<Order> OrderCancelled;
        public event Action<Order, Order, FillFlags, Price, Volume> OrderFilled;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="currencyPair"></param>
        public LimitOrderBook(string currencyPair)
        {
            _currencyPair = currencyPair;

            _bids = new OrderList(currencyPair, OrderSide.Buy);
            _asks = new OrderList(currencyPair, OrderSide.Sell);
        }

        #region Methods

        /// <summary>
        /// Matches the incoming order to the list of available orders
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool AddOrder(Order order)
        {
            // If order is valid, otherwise, IsValid method updates order state as rejected and sends back notification to the client
            if (IsValid(order))
            {
                return PlaceOrder(order);
            }
            
            return false;
        }

        /// <summary>
        /// Place a new order on the order book
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool PlaceOrder(Order order)
        {
            switch (order.OrderSide)
            {
                case OrderSide.Sell:
                    return MatchSellOrder(order);

                case OrderSide.Buy:
                    return MatchBuyOrder(order);
            }
            return false;
        }

        /// <summary>
        /// Tries to match the incoming Sell Order with the Bids present
        /// </summary>
        /// <param name="sellOrder"></param>
        /// <returns></returns>
        public bool MatchSellOrder(Order sellOrder)
        {
            if (_bids.Any())
            {
                return MatchOrder(sellOrder, _bids, _asks);
            }
            // If there is no Bids on the book, then raise the event that this order has been accepted and been added to the Asks
            RaiseOrderAcceptedEvent(sellOrder, 0, 0);
            _asks.Add(sellOrder);
            // Didnt match
            return false;
        }

        /// <summary>
        /// Tries to match the incoming Buy Order with the Asks present
        /// </summary>
        /// <param name="buyOrder"></param>
        /// <returns></returns>
        public bool MatchBuyOrder(Order buyOrder)
        {
            if (_asks.Any())
            {
                return MatchOrder(buyOrder, _asks, _bids);
            }
            // If there are no Asks on the book, then raise the event that this order has been accepted and added to the Bids
            RaiseOrderAcceptedEvent(buyOrder, 0, 0);
            _bids.Add(buyOrder);
            // Didn't match
            return false;
        }

        /// <summary>
        /// Matches the order of either side with the opposite side's orders placed in the opposite side's list
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="oppositeSideList">The list for containing orders for the opposite side, e.g., if the order is sell
        /// then this list is Bids' list</param>
        /// <param name="inboundSideList">The list of the side corresponding to the current order, e.g., if the order is sell
        /// then the list is Asks' list</param>
        /// <returns></returns>
        public bool MatchOrder(Order order, OrderList oppositeSideList, OrderList inboundSideList)
        {
            bool sendOrderAcceptedEvent = true;
            bool matched = false;
            List<Order> ordersToRemove = null;
            foreach (Order matchingOrder in oppositeSideList)
            {
                if (Matched(order.Price, matchingOrder.Price, order.OrderSide, order.OrderType))
                {
                    matched = true;
                    CrossOrders(order, matchingOrder, sendOrderAcceptedEvent);
                    sendOrderAcceptedEvent = false;

                    //decimal currentVolume = order.OpenQuantity.Value - matchingOrder.OpenQuantity.Value;

                    // If the inbound Order's volume exceeds the opposite side order's volume present in the list
                    if (matchingOrder.OrderState == OrderState.Complete)
                    {
                        if (ordersToRemove == null)
                        {
                            ordersToRemove = new List<Order>();
                        }
                        ordersToRemove.Add(matchingOrder);
                        // As there is no open quantity left in the current order, it will be removed from the Orderbook
                        //oppositeSideList.Remove(matchingOrder);

                        if (OrderBookChanged != null)
                        {
                            OrderBookChanged(this);
                        }
                        // Let the loop continue as their is open quantity yet to be filled
                    }
                    // If the inbound Order's volume is less than the opposite side Order's volume in the list
                    if (order.OrderState == OrderState.Complete)
                    {
                        // The matched order's open quantity is yet remaining, but the original volume will stay the same
                        break;
                    }
                }
                if (order.OrderSide == OrderSide.Buy && matchingOrder.Price.Value > order.Price.Value)
                {
                    break;
                }
                // If the first bid on the book does not satisfy the price, then termintate the loop as it won't be beneficial
                // looking for a match ahead
                else if (order.OrderSide == OrderSide.Sell && matchingOrder.Price.Value < order.Price.Value)
                {
                    break;
                }
            }
            if (ordersToRemove != null && ordersToRemove.Any())
            {
                foreach (var order1 in ordersToRemove)
                {
                    oppositeSideList.Remove(order1);
                }
            }

            // If there is still quantity left to be filled in this order, add it to it's side's order book
            if (order.OpenQuantity.Value > 0)
            {
                if (sendOrderAcceptedEvent)
                {
                    RaiseOrderAcceptedEvent(order, 0, 0);
                }
                inboundSideList.Add(order);
                if (OrderBookChanged != null)
                {
                    OrderBookChanged(this);
                }
            }
            return matched;
        }

        /// <summary>
        /// Validates if the prices match for the criteria specified for each level
        /// </summary>
        /// <param name="inboundPrice"></param>
        /// <param name="currentPrice"></param>
        /// <param name="orderSide"></param>
        /// <returns></returns>
        public bool Matched(Price inboundPrice, Price currentPrice, OrderSide orderSide, OrderType orderType)
        {
            if (orderType == OrderType.Limit)
            {
                switch (orderSide)
                {
                    case OrderSide.Buy:
                        if (inboundPrice >= currentPrice)
                        {
                            return true;
                        }
                        return false;
                    case OrderSide.Sell:
                        if (inboundPrice <= currentPrice)
                        {
                            return true;
                        }
                        return false;
                }
            }
            else if(orderType == OrderType.Market && inboundPrice != null && inboundPrice.Value == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cancel the order
        /// </summary>
        public bool CancelOrder(OrderId orderId)
        {
            Order order = null;
            bool found = false;
            order = _asks.FindOrder(orderId);
            if (order != null)
            {
                _asks.Remove(order);
                found = true;
            }

            if (!found)
            {
                order = _bids.FindOrder(orderId);
                if (order != null)
                {
                    _bids.Remove(order);
                }
            }
            if (order != null)
            {
                order.Cancelled();
                if (OrderCancelled != null)
                {
                    OrderCancelled(order);
                }
                if (OrderChanged != null)
                {
                    OrderChanged(order);
                }
                if (OrderBookChanged != null)
                {
                    OrderBookChanged(this);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates the Trade after a cross between two orders
        /// </summary>
        /// <returns></returns>
        private Trade GenerateTrade(decimal executionPrice, 
            decimal executedQuantity, Order matchedOrder, Order inboundOrder)
        {
           // Trade trade = new Trade(matchedOrder.CurrencyPair, new Price(executionPrice), new Volume(executedQuantity),
             //   DateTime.Now, matchedOrder, inboundOrder);
            Trade trade = TradeFactory.GenerateTrade(matchedOrder.CurrencyPair, new Price(executionPrice),
                new Volume(executedQuantity),
                matchedOrder, inboundOrder);
            
            return trade;
        }

        /// <summary>
        /// Validates wheterh the order is valid or not
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool IsValid(Order order)
        {
            if (order.Volume.Value == 0)
            {
                order.Rejected();
                if (OrderChanged != null)
                {
                    OrderChanged(order);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the price for the inbound order, can depend on various circumstances that can be added later
        /// </summary>
        /// <returns></returns>
        public Price SortPrice(Order order)
        {
            return order.Price;
        }

        #region Order Updating methods

        /// <summary>
        /// Change quantity for an order
        /// </summary>
        public void ChangeQuantity(Order order, Volume newVolume)
        {
            AssertionConcern.AssertGreaterThanZero(newVolume.Value, "New volume for the order is less than zero");
            order.UpdateVolume(newVolume);
        }

        /// <summary>
        /// Add a new order to its corresponding depth level
        /// </summary>
        /// <param name="order"></param>
        public void AddOrderToDepth(Order order)
        {
            if (OrderChanged != null)
            {
                OrderChanged(order);
            }
        }

        public void UpdateOrderInDepth(Order order, Volume newVolume)
        {
            ChangeQuantity(order, newVolume);
            if (OrderChanged != null)
            {
                // The depthOrderBook does not need to get this new volume, the order already contains the new volume
                OrderChanged(order);
            }
        }

        /// <summary>
        /// Raise Order Accepted event
        /// </summary>
        /// <param name="order"></param>
        /// <param name="matchedPrice"></param>
        /// <param name="matchedVolume"></param>
        private void RaiseOrderAcceptedEvent(Order order, decimal matchedPrice, decimal matchedVolume)
        {
            order.Accepted();
            // This event will be handled by the Depth Order book
            if (OrderAccepted != null)
            {
                OrderAccepted(order, new Price(matchedPrice), new Volume(matchedVolume));
            }
            // This event will be handled by the OrderListener, which will then dispatch the event on the ouput disruptor
            // to be journaled and then sent to the read side
            if (OrderChanged != null)
            {
                OrderChanged(order);
            }
        }

        /// <summary>
        /// Update the order according to the fill
        /// </summary>
        /// <param name="matchedOrder"></param>
        /// <param name="inboundOrder"> </param>
        /// <param name="sendOrderAcceptedEvent"> </param>
        public void CrossOrders(Order inboundOrder, Order matchedOrder, bool sendOrderAcceptedEvent)
        {
            decimal filledQuantity = Math.Min(inboundOrder.OpenQuantity.Value, matchedOrder.OpenQuantity.Value);
            decimal filledPrice = matchedOrder.Price.Value;

            if (inboundOrder.OrderType == OrderType.Market && inboundOrder.Price.Value == 0)
            {
                filledPrice = inboundOrder.Price.Value;
            }

            // Send Order accepted notification to subscribers
            if (sendOrderAcceptedEvent)
            {
                RaiseOrderAcceptedEvent(inboundOrder, filledPrice, filledQuantity);
            }

            Price inboundFilledCost = new Price(filledQuantity * filledPrice);
            inboundOrder.Fill(new Volume(filledQuantity), inboundFilledCost);

            Price matchedFilledCost = new Price(filledQuantity * filledPrice);
            matchedOrder.Fill(new Volume(filledQuantity), matchedFilledCost);

            FillFlags fillFlags = FillFlags.NetitherFilled;
            if (matchedOrder.OpenQuantity.Value == 0 && inboundOrder.OpenQuantity.Value == 0)
            {
                fillFlags = FillFlags.BothFilled;
            }
            else if (matchedOrder.OpenQuantity.Value == 0)
            {
                fillFlags = FillFlags.MatchedFilled;
            }
            else if (inboundOrder.OpenQuantity.Value == 0)
            {
                fillFlags = FillFlags.InboundFilled;
            }
            OrderFillEventsRaise(inboundOrder, matchedOrder, fillFlags, filledPrice, filledQuantity);

            // Create trade. The least amount of the two orders will be the trade's executed volume
            Trade trade = GenerateTrade(filledPrice, filledQuantity, matchedOrder, inboundOrder);
            
            if (TradeExecuted != null)
            {
                TradeExecuted(trade);
            }
        }

        /// <summary>
        /// Called when the orders will cross and get filled, and will raise the events for order fill and order changed
        /// </summary>
        /// <param name="inboundOrder"></param>
        /// <param name="matchedOrder"></param>
        /// <param name="fillFlags"></param>
        /// <param name="filledPrice"></param>
        /// <param name="filledQuantity"></param>
        private void OrderFillEventsRaise(Order inboundOrder, Order matchedOrder, FillFlags fillFlags, decimal filledPrice,
            decimal filledQuantity)
        {
            // Event will be handled in the Depth Order book
            if (OrderFilled != null)
            {
                OrderFilled(inboundOrder, matchedOrder, fillFlags, new Price(filledPrice), new Volume(filledQuantity));
            }
            // Event for the incoming order from the user that got filled
            // This event will be handled by the OrderListener, which will then dispatch the event on the ouput disruptor
            // to be journaled and then sent to the read side
            if (OrderChanged != null)
            {
                OrderChanged(inboundOrder);
            }
            // Event for the order on order book that got matched with the incoming order
            if (OrderChanged != null)
            {
                OrderChanged(matchedOrder);
            }
        }

        #endregion Order Updating methods

        #endregion

        #region Properties

        /// <summary>
        /// Currency Pair of this book
        /// </summary>
        public string CurrencyPair
        {
            get { return _currencyPair; }
            private set { _currencyPair = value; }
        }

        /// <summary>
        /// Listens to the Trades that are executed by the Order Book
        /// </summary>
        public TradeListener TradeListener
        {
            get
            {
                return _tradeListener;
            }
            set
            {
                AssertionConcern.AssertArgumentNotNull(value, "TradeListener provided is equal to null.");
                _tradeListener = value;
                if (TradeExecuted == null)
                {
                    TradeExecuted += _tradeListener.OnTrade;
                }
            }
        }

        /// <summary>
        /// Listens to the changes in the states of Orders
        /// </summary>
        public OrderListener OrderListener
        {
            get
            {
                return _orderListener;
            } 
            set
            {
                AssertionConcern.AssertArgumentNotNull(value, "OrderListener provided is equal to null.");
                _orderListener = value;
                if (OrderChanged == null)
                {
                    OrderChanged += _orderListener.OnOrderChanged;
                }
            }
        }

        /// <summary>
        /// Listens to the changes in the states of this OrderBook
        /// </summary>
        public OrderBookListener OrderBookListener
        {
            get
            {
                return _orderBookListener;
            }
            set
            {
                AssertionConcern.AssertArgumentNotNull(value, "OrderBookListener provided is equal to null.");
                _orderBookListener = value;
                if (OrderBookChanged == null)
                {
                    OrderBookChanged += _orderBookListener.OnOrderBookChanged;
                }
            }
        }

        /// <summary>
        /// The Bids list. Methods to add, modify, remove are internal so cannot be called outside the assembly
        /// </summary>
        public OrderList Bids
        {
            get { return _bids; }
            private set { _bids = value; }
        }

        /// <summary>
        /// The Asks list. Methods to add, modify, remove are internal so cannot be called outside the assembly
        /// </summary>
        public OrderList Asks
        {
            get { return _asks; }
            private set { _asks = value; }
        }

        /// <summary>
        /// The number of Bids present in the Bids list
        /// </summary>
        public int BidCount
        {
            get { return _bids.Count(); }
        }

        /// <summary>
        /// The number of Asks present in the Bids list
        /// </summary>
        public int AskCount
        {
            get { return _asks.Count(); }
        }

        #endregion Properties
    }
}
