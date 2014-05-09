﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
using CoinExchange.Common.Domain.Model;
using CoinExchange.Trades.Application.OrderServices.Representation;
using CoinExchange.Trades.Domain.Model.OrderAggregate;
using CoinExchange.Trades.Domain.Model.OrderMatchingEngine;
using CoinExchange.Trades.Domain.Model.Services;
using CoinExchange.Trades.Infrastructure.Persistence.RavenDb;
using CoinExchange.Trades.Port.Adapter.Rest.DTOs.Order;
using CoinExchange.Trades.Port.Adapter.Rest.Resources;
using Disruptor;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

namespace CoinExchange.Trades.Port.Adapter.Rest.IntegrationTests
{
    /// <summary>
    /// Test cases for the Trades Controller
    /// </summary>
    class TradesControllerTestCases
    {
        [Test]
        [Category("Integration")]
        public void GetAllTradesTest_TestsTheMethodThatWillGetAllTradesForACurrencypair_AssertsTheValuesOfTheFetchedTrades()
        {
            // Get the context
            IApplicationContext applicationContext = ContextRegistry.GetContext();
            Exchange exchange = new Exchange();
            IEventStore inputEventStore = new RavenNEventStore(Constants.INPUT_EVENT_STORE);
            IEventStore outputEventStore = new RavenNEventStore(Constants.OUTPUT_EVENT_STORE);
            Journaler inputJournaler = new Journaler(inputEventStore);
            Journaler outputJournaler = new Journaler(outputEventStore);
            InputDisruptorPublisher.InitializeDisruptor(new IEventHandler<InputPayload>[] { exchange, inputJournaler });
            OutputDisruptor.InitializeDisruptor(new IEventHandler<byte[]>[] { outputJournaler });

            // Get the instance through Spring configuration
            TradeController tradeController = (TradeController)applicationContext["TradeController"];

            // Get the instance through Spring configuration
            OrderController orderController = (OrderController)applicationContext["OrderController"];

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            orderController.CreateOrder(new CreateOrderParam()
            {
                Pair = "BTCUSD", Price = 491, Volume = 100, Side = "buy", Type = "limit"
            });

            manualResetEvent.Reset();
            manualResetEvent.WaitOne(2000);
            orderController.CreateOrder(new CreateOrderParam()
                                        {
                                            Pair = "BTCUSD", Price = 492, Volume = 300, Side = "buy", Type = "limit"
                                        });

            manualResetEvent.Reset();
            manualResetEvent.WaitOne(2000);

            orderController.CreateOrder(new CreateOrderParam()
                                        {
                                            Pair = "BTCUSD", Price = 493, Volume = 1000, Side = "buy", Type = "limit"
                                        });

            orderController.CreateOrder(new CreateOrderParam()
                                        {
                                            Pair = "BTCUSD", Price = 491, Volume = 900, Side = "sell", Type = "limit"
                                        });

            manualResetEvent.Reset();
            manualResetEvent.WaitOne(2000);

            orderController.CreateOrder(new CreateOrderParam()
                                        {
                                            Pair = "BTCUSD", Price = 492, Volume = 800, Side = "sell", Type = "limit"
                                        });

            manualResetEvent.Reset();
            manualResetEvent.WaitOne(2000);

            orderController.CreateOrder(new CreateOrderParam()
                                        {
                                            Pair = "BTCUSD", Price = 493, Volume = 700, Side = "sell", Type = "limit"
                                        });

            manualResetEvent.Reset();
            manualResetEvent.WaitOne(6000);
            IHttpActionResult httpActionResult = tradeController.RecentTrades("BTCUSD", "");
            OkNegotiatedContentResult<IList<object>> okResponse = (OkNegotiatedContentResult<IList<object>>) httpActionResult;

            IList<object> objectList = (IList<object>)okResponse.Content;
            List<object[]> newObjectsList = new List<object[]>();

            for (int i = 0; i < objectList.Count; i++)
            {
                object[] objects = objectList[0] as object[];
                newObjectsList.Add(objects);
            }

            Assert.AreEqual(100, newObjectsList[0][1]);
            Assert.AreEqual(491, newObjectsList[0][2]);

            Assert.AreEqual(300, newObjectsList[0][1]);
            Assert.AreEqual(492, newObjectsList[0][2]);
        }

        /// <summary>
        /// Tests the functionality to fecth all the open orders
        /// </summary>
        [Test]
        public void GetOpenOrdersTestCase()
        {
            //OrderController orderController = new OrderController();
            //IHttpActionResult httpActionResult = orderController.QueryOpenOrders(null);

            //// The result is and should be returned as IHttpActionResult, which contains content as well as response codes for
            //// Http response messages sent to the client.  If it is not null, menas the request was successful
            //Assert.IsNotNull(httpActionResult);
            //Assert.AreEqual(httpActionResult.GetType(), typeof(OkNegotiatedContentResult<List<Order>>));
            //OkNegotiatedContentResult<List<Order>> okResponseMessage = (OkNegotiatedContentResult<List<Order>>)httpActionResult;

            //// If the response message contains content and its count is greater than 0, our test is successful
            //Assert.IsNotNull(okResponseMessage.Content);
            //Assert.GreaterOrEqual(okResponseMessage.Content.Count(), 1, "Count of the contents in the OK response message");
        }
    }
}
