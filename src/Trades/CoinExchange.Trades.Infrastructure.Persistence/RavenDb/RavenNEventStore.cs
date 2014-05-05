﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CoinExchange.Common.Domain.Model;
using CoinExchange.Trades.Domain.Model.OrderAggregate;
using CoinExchange.Trades.Domain.Model.Services;
using CoinExchange.Trades.Domain.Model.TradeAggregate;
using NEventStore;
using NEventStore.Dispatcher;

namespace CoinExchange.Trades.Infrastructure.Persistence.RavenDb
{
    /// <summary>
    /// Raven NEvent Store
    /// </summary>
    public class RavenNEventStore : IEventStore
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid StreamId = Guid.NewGuid();
        private static IStoreEvents _store;
        private static IEventStream _stream;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RavenNEventStore(string eventStore)
        {
            _store = GetInitializedEventStore(new ReceiveCommit(),eventStore);
            _stream = _store.OpenStream(StreamId, 0, int.MaxValue);
        }

        /// <summary>
        /// Store the Event
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        public bool StoreEvent(object blob)
        {
            try
            {
                OpenOrCreateStream(blob);    
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initialize RavenDB NEventStore
        /// </summary>
        /// <returns></returns>
        private static IStoreEvents GetInitializedEventStore(IDispatchCommits commits,string eventStore)
        {
            return Wireup.Init()
                .UsingRavenPersistence(Constants.RAVEN_DB_CONNECTIONSTRING_NAME)
                .DefaultDatabase(eventStore)
                .UsingAsynchronousDispatchScheduler(commits)
                .Build();
        }

        /// <summary>
        /// Write event to the stream(RavenDB collection)
        /// </summary>
        /// <param name="event"></param>
        private static void OpenOrCreateStream(object @event)
        {
            _stream.Add(new EventMessage {Body = @event});
            _stream.CommitChanges(Guid.NewGuid());
            if (@event is Order)
            {
                Log.Debug("New order stored in Event Store. ID: " + (@event as Order).OrderId.Id + " | Order state: "
                          + (@event as Order).OrderState);
            }
        }

        /// <summary>
        /// Get Event from collection
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetEvent(string id)
        {
            List<EventMessage> collection;
            using (var stream = _store.OpenStream(StreamId, 0, int.MaxValue))
            {
                collection = stream.CommittedEvents.ToList();
                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i].Body is Order)
                    {
                        Order order = collection[i].Body as Order;
                        if (order.OrderId.Id.ToString(CultureInfo.InvariantCulture).Equals(id))
                        {
                            return order;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all the orders placed residing inside the EventStore
        /// </summary>
        /// <returns></returns>
        public List<Order> GetAllOrders()
        {
            List<Order> orders = new List<Order>();
            List<EventMessage> collection;
            collection = _stream.CommittedEvents.ToList();
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Body is Order)
                {
                    Order order = collection[i].Body as Order;
                    orders.Add(order);
                }
            }
            Log.Debug("Number of orders fetched from Event Store: " + orders.Count);
            if (!orders.Any())
            {
                orders = null;
            }
            return orders;
        }

        /// <summary>
        /// Removes all order from the NEventStore
        /// </summary>
        public void RemoveAllEvents()
        {
            _store.Advanced.Purge();
            _stream.CommittedEvents.Clear();
        }

        /// <summary>
        /// Gets the orders based on the specified CurrencyPair
        /// </summary>
        /// <returns></returns>
        public List<Order> GetOrdersByCurrencyPair(string currencyPair)
        {
            List<Order> orders = new List<Order>();
            List<EventMessage> collection;
            collection = _stream.CommittedEvents.ToList();
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Body is Order)
                {
                    Order order = collection[i].Body as Order;
                    if (order.CurrencyPair == currencyPair)
                    {
                        orders.Add(order);
                    }
                }
            }
            Log.Debug("Number of orders fetched from Event Store: " + orders.Count);
            if (!orders.Any())
            {
                orders = null;
            }
            return orders;
        }

        /// <summary>
        /// Get trade Event from collection by order id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<Trade> GetTradeEventsFromOrderId(string id)
        {
            List<EventMessage> collection;
            List<Trade> trades=new List<Trade>();
            using (var stream = _store.OpenStream(StreamId, 0, int.MaxValue))
            {
                collection = stream.CommittedEvents.ToList();
                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i].Body is Trade)
                    {
                        Trade trade = collection[i].Body as Trade;
                        if (trade.BuyOrder.OrderId.Id.ToString().Equals(id)||trade.SellOrder.OrderId.Id.ToString().Equals(id))
                        {
                            trades.Add(trade);
                        }
                    }
                }
            }
            return trades;
        }
        
        public IList<object> GetAllEvents()
        {
            List<object> events=new List<object>();
            List<EventMessage> collection;
            collection = _stream.CommittedEvents.ToList();
            for (int i = 0; i < collection.Count; i++)
            {
                events.Add(collection[i].Body);
            }
            return events;
        }
    }
}
