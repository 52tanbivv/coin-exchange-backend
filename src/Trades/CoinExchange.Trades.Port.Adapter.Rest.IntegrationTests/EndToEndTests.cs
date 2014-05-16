﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CoinExchange.Common.Domain.Model;
using CoinExchange.Common.Tests;
using CoinExchange.Trades.Application.OrderServices.Representation;
using CoinExchange.Trades.Domain.Model.OrderAggregate;
using CoinExchange.Trades.Domain.Model.OrderMatchingEngine;
using CoinExchange.Trades.Domain.Model.Services;
using CoinExchange.Trades.Infrastructure.Persistence.RavenDb;
using CoinExchange.Trades.Port.Adapter.Rest.DTOs.Order;
using CoinExchange.Trades.Port.Adapter.Rest.DTOs.Trade;
using CoinExchange.Trades.Port.Adapter.Rest.Resources;
using CoinExchange.Trades.ReadModel.DTO;
using CoinExchange.Trades.ReadModel.MemoryImages;
using Disruptor;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spring.Context;
using Spring.Context.Support;

namespace CoinExchange.Trades.Port.Adapter.Rest.IntegrationTests
{
    [TestFixture]
    public class EndToEndTests
    {
        private DatabaseUtility _databaseUtility;
        private IEventStore inputEventStore;
        private IEventStore outputEventStore;
        private Journaler inputJournaler;
        private Journaler outputJournaler;


        [SetUp]
        public new void SetUp()
        {
            var connection = ConfigurationManager.ConnectionStrings["MySql"].ToString();
            _databaseUtility = new DatabaseUtility(connection);
            _databaseUtility.Create();
            _databaseUtility.Populate();
            inputEventStore = new RavenNEventStore(Constants.INPUT_EVENT_STORE);
            outputEventStore = new RavenNEventStore(Constants.OUTPUT_EVENT_STORE);
            inputJournaler = new Journaler(inputEventStore);
            outputJournaler = new Journaler(outputEventStore);
            Exchange exchange = new Exchange();
            InputDisruptorPublisher.InitializeDisruptor(new IEventHandler<InputPayload>[] { exchange, inputJournaler });
            OutputDisruptor.InitializeDisruptor(new IEventHandler<byte[]>[] { outputJournaler });
        }

        [TearDown]
        public new void TearDown()
        {
            _databaseUtility.Create();
            InputDisruptorPublisher.Shutdown();
            OutputDisruptor.ShutDown();
            inputEventStore.RemoveAllEvents();
            outputEventStore.RemoveAllEvents();
        }

        [Test]
        [Category("Integration")]
        public void Scenario1Test_TestsScenario1AndItsOutcome_VerifiesThroughMarketDataOrderAndTradesResults()
        {
            // Get the context
            IApplicationContext applicationContext = ContextRegistry.GetContext();
            //Exchange exchange = new Exchange();
            //IEventStore inputEventStore = new RavenNEventStore(Constants.INPUT_EVENT_STORE);
            //IEventStore outputEventStore = new RavenNEventStore(Constants.OUTPUT_EVENT_STORE);
            //Journaler inputJournaler = new Journaler(inputEventStore);
            //Journaler outputJournaler = new Journaler(outputEventStore);
            //InputDisruptorPublisher.InitializeDisruptor(new IEventHandler<InputPayload>[] { exchange, inputJournaler });
            //OutputDisruptor.InitializeDisruptor(new IEventHandler<byte[]>[] { outputJournaler });

            // Get the instance through Spring configuration
            OrderController orderController = (OrderController)applicationContext["OrderController"];
            TradeController tradeController = (TradeController)applicationContext["TradeController"];
            MarketController marketController = (MarketController)applicationContext["MarketController"];
            orderController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            orderController.Request.Headers.Add("Auth", "123456789");
            tradeController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            tradeController.Request.Headers.Add("Auth", "123456789");
            marketController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            marketController.Request.Headers.Add("Auth", "123456789");
            Scenario1OrderCreation(orderController);

            // ------------------------------- Order Book ------------------------------
            IHttpActionResult orderBookResponse = marketController.GetOrderBook("XBTUSD");

            OkNegotiatedContentResult<Tuple<OrderRepresentationList, OrderRepresentationList>> okOrderBookResponse =
                (OkNegotiatedContentResult<Tuple<OrderRepresentationList, OrderRepresentationList>>) orderBookResponse;

            Tuple<OrderRepresentationList, OrderRepresentationList> orderBook = okOrderBookResponse.Content;

            // Item1 = Bid Book, Item1[i].Item1 = Volume of 'i' Bid, Item1[i].Item2 = Price of 'i' Bid
            Assert.AreEqual(5, orderBook.Item1.ToList()[0].Item1);
            Assert.AreEqual(250, orderBook.Item1.ToList()[0].Item2);
            Assert.AreEqual(2, orderBook.Item1.ToList()[1].Item1);
            Assert.AreEqual(250, orderBook.Item1.ToList()[1].Item2);

            // Item2 = Ask Book, Item2[i].Item1 = Volume of Ask at index 'i', Item2[i].Item2 = Price of Ask at index 'i'
            Assert.AreEqual(0, orderBook.Item2.Count());
            // ------------------------------- Order Book ------------------------------
            IHttpActionResult depthResponse = marketController.GetDepth("XBTUSD");

            OkNegotiatedContentResult<Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]>> okDepth
                = (OkNegotiatedContentResult<Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]>>)depthResponse;

            Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]> depth = okDepth.Content;
            // Item1 = Bid Book, Item1.Item1 = Aggregated Volume, Item1.Item2 = Price, Item1.Item3 = Number of Orders
            Assert.AreEqual(7, depth.Item1[0].Item1);
            Assert.AreEqual(250, depth.Item1[0].Item2);
            Assert.AreEqual(2, depth.Item1[0].Item3);
            Assert.IsNull(depth.Item1[1]);
            Assert.IsNull(depth.Item1[2]);
            Assert.IsNull(depth.Item1[3]);
            Assert.IsNull(depth.Item1[4]);

            // Item2 = Bid Book, Item2.Item1 = Aggregated Volume, Item2.Item2 = Price, Item2.Item3 = Number of Orders
            Assert.IsNull(depth.Item2[0]);
            Assert.IsNull(depth.Item2[1]);
            Assert.IsNull(depth.Item2[2]);
            Assert.IsNull(depth.Item2[3]);
            Assert.IsNull(depth.Item2[4]);

            //------------------- Open Orders -------------------------       
            IHttpActionResult openOrdersResponse = GetOpenOrders(orderController);
            OkNegotiatedContentResult<List<OrderReadModel>> okOpenOrdersResponse = (OkNegotiatedContentResult<List<OrderReadModel>>)openOrdersResponse;
            List<OrderReadModel> openOrders = okOpenOrdersResponse.Content;

            Assert.AreEqual(2, openOrders.Count);
            // First Open Order
            Assert.AreEqual(250, openOrders[0].Price);
            Assert.AreEqual(10, openOrders[0].Volume);
            Assert.AreEqual(5, openOrders[0].VolumeExecuted);
            Assert.AreEqual(5, openOrders[0].OpenQuantity);
            Assert.AreEqual("Limit", openOrders[0].OrderType);
            Assert.AreEqual("Buy", openOrders[0].OrderSide);
            Assert.AreEqual("PartiallyFilled", openOrders[0].Status);

            // Second Open Order
            Assert.AreEqual(250, openOrders[1].Price);
            Assert.AreEqual(2, openOrders[1].Volume);
            Assert.AreEqual(0, openOrders[1].VolumeExecuted);
            Assert.AreEqual(2, openOrders[1].OpenQuantity);
            Assert.AreEqual("Limit", openOrders[1].OrderType);
            Assert.AreEqual("Buy", openOrders[1].OrderSide);
            Assert.AreEqual("Accepted", openOrders[1].Status);
            //------------------- Open Orders -------------------------

            //------------------- Closed Orders -------------------------
            IHttpActionResult closedOrdersResponse = GetClosedOrders(orderController);
            OkNegotiatedContentResult<List<OrderReadModel>> okClosedOrdersResponse = (OkNegotiatedContentResult<List<OrderReadModel>>)closedOrdersResponse;
            List<OrderReadModel> closedOrders = okClosedOrdersResponse.Content;
            Assert.AreEqual(252, closedOrders[0].Price);
            Assert.AreEqual(5, closedOrders[0].Volume);
            Assert.AreEqual(5, closedOrders[0].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[0].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[0].OrderType);
            Assert.AreEqual("Sell", closedOrders[0].OrderSide);
            Assert.AreEqual("Complete", closedOrders[0].Status);

            Assert.AreEqual(0, closedOrders[1].Price);
            Assert.AreEqual(3, closedOrders[1].Volume);
            Assert.AreEqual(3, closedOrders[1].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[1].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[1].OrderType);
            Assert.AreEqual("Buy", closedOrders[1].OrderSide);
            Assert.AreEqual("Complete", closedOrders[1].Status);

            Assert.AreEqual(253, closedOrders[2].Price);
            Assert.AreEqual(2, closedOrders[2].Volume);
            Assert.AreEqual(2, closedOrders[2].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[2].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[2].OrderType);
            Assert.AreEqual("Buy", closedOrders[2].OrderSide);
            Assert.AreEqual("Complete", closedOrders[2].Status);

            Assert.AreEqual(0, closedOrders[3].Price);
            Assert.AreEqual(5, closedOrders[3].Volume);
            Assert.AreEqual(5, closedOrders[3].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[3].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[3].OrderType);
            Assert.AreEqual("Sell", closedOrders[3].OrderSide);
            Assert.AreEqual("Complete", closedOrders[3].Status);
            //------------------- Closed Orders -------------------------

            //------------------- Trades -------------------------

            IHttpActionResult tradesResponse = GetTrades(tradeController);
            OkNegotiatedContentResult<object> okTradeResponse = (OkNegotiatedContentResult<object>) tradesResponse;
            IList<object> tradesintermediateList = (IList<object>)okTradeResponse.Content;
            IList<object[]> trades = new List<object[]>();

            for (int i = 0; i < tradesintermediateList.Count; i++)
            {
                object[] objects = tradesintermediateList[i] as object[];
                trades.Add(objects);
            }

            // This call return list of object, so we have to explicitly check values within elements
            Assert.AreEqual(252, trades[0][2]);// Price
            Assert.AreEqual(3, trades[0][3]);// Volume
            Assert.AreEqual("XBTUSD", trades[0][4]);

            Assert.AreEqual(252, trades[1][2]);
            Assert.AreEqual(2, trades[1][3]);
            Assert.AreEqual("XBTUSD", trades[0][4]);

            Assert.AreEqual(250, trades[2][2]);
            Assert.AreEqual(5, trades[2][3]);
            Assert.AreEqual("XBTUSD", trades[0][4]);
            //------------------- Trades -------------------------
        }

        [Test]
        [Category("Integration")]
        public void Scenario2Test_TestsScenario2AndItsOutcome_VerifiesThroughMarketDataOrderAndTradesResults()
        {
            // Get the context
            IApplicationContext applicationContext = ContextRegistry.GetContext();
            //Exchange exchange = new Exchange();
            //IEventStore inputEventStore = new RavenNEventStore(Constants.INPUT_EVENT_STORE);
            //IEventStore outputEventStore = new RavenNEventStore(Constants.OUTPUT_EVENT_STORE);
            //Journaler inputJournaler = new Journaler(inputEventStore);
            //Journaler outputJournaler = new Journaler(outputEventStore);
            //InputDisruptorPublisher.InitializeDisruptor(new IEventHandler<InputPayload>[] { exchange, inputJournaler });
            //OutputDisruptor.InitializeDisruptor(new IEventHandler<byte[]>[] { outputJournaler });

            // Get the instance through Spring configuration
            OrderController orderController = (OrderController)applicationContext["OrderController"];
            TradeController tradeController = (TradeController)applicationContext["TradeController"];
            MarketController marketController = (MarketController)applicationContext["MarketController"];
            orderController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            orderController.Request.Headers.Add("Auth", "123456789");
            tradeController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            tradeController.Request.Headers.Add("Auth", "123456789");
            marketController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            marketController.Request.Headers.Add("Auth", "123456789");
            Scenario2OrderCreation(orderController);

            // ------------------------------- Order Book ------------------------------
            IHttpActionResult orderBookResponse = marketController.GetOrderBook("XBTUSD");

            OkNegotiatedContentResult<Tuple<OrderRepresentationList, OrderRepresentationList>> okOrderBookResponse =
                (OkNegotiatedContentResult<Tuple<OrderRepresentationList, OrderRepresentationList>>)orderBookResponse;

            Tuple<OrderRepresentationList, OrderRepresentationList> orderBook = okOrderBookResponse.Content;

            // Item1 = Bid Book, Item1[i].Item1 = Volume of 'i' Bid, Item1[i].Item2 = Price of 'i' Bid
            Assert.AreEqual(1, orderBook.Item1.Count());
            Assert.AreEqual(1, orderBook.Item1.ToList()[0].Item1);
            Assert.AreEqual(245, orderBook.Item1.ToList()[0].Item2);

            // Item2 = Bid Book, Item2[i].Item1 = Volume of Ask at index 'i', Item2[i].Item2 = Price of Bid at index 'i'
            Assert.AreEqual(0, orderBook.Item2.Count());
            // ------------------------------------------------------------------------

            // --------------------------------- Depth ---------------------------------
            IHttpActionResult depthResponse = marketController.GetDepth("XBTUSD");

            OkNegotiatedContentResult<Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]>> okDepth
                = (OkNegotiatedContentResult<Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]>>)depthResponse;

            Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]> depth = okDepth.Content;

            // Item1 = Bid Book, Item1.Item1 = Aggregated Volume, Item1.Item2 = Price, Item1.Item3 = Number of Orders
            Assert.AreEqual(1, depth.Item1[0].Item1);
            Assert.AreEqual(245, depth.Item1[0].Item2);
            Assert.AreEqual(1, depth.Item1[0].Item3);
            Assert.IsNull(depth.Item1[1]);
            Assert.IsNull(depth.Item1[2]);
            Assert.IsNull(depth.Item1[3]);
            Assert.IsNull(depth.Item1[4]);

            // Item2 = Bid Book, Item2.Item1 = Aggregated Volume, Item2.Item2 = Price, Item2.Item3 = Number of Orders
            Assert.IsNull(depth.Item2[0]);
            Assert.IsNull(depth.Item2[1]);
            Assert.IsNull(depth.Item2[2]);
            Assert.IsNull(depth.Item2[3]);
            Assert.IsNull(depth.Item2[4]);

            // -----------------------------------------------------------------------

            //------------------------- Open Orders ----------------------------------
            IHttpActionResult openOrdersResponse = GetOpenOrders(orderController);
            OkNegotiatedContentResult<List<OrderReadModel>> okOpenOrdersResponse = (OkNegotiatedContentResult<List<OrderReadModel>>)openOrdersResponse;
            List<OrderReadModel> openOrders = okOpenOrdersResponse.Content;

            Assert.AreEqual(1, openOrders.Count);
            // First Open Order
            Assert.AreEqual(245, openOrders[0].Price);
            Assert.AreEqual(8, openOrders[0].Volume);
            Assert.AreEqual(7, openOrders[0].VolumeExecuted);
            Assert.AreEqual(1, openOrders[0].OpenQuantity);
            Assert.AreEqual("Limit", openOrders[0].OrderType);
            Assert.AreEqual("Buy", openOrders[0].OrderSide);
            Assert.AreEqual("PartiallyFilled", openOrders[0].Status);
            //---------------------------------------------------------------------

            //-------------------------- Closed Orders ----------------------------
            IHttpActionResult closedOrdersResponse = GetClosedOrders(orderController);
            OkNegotiatedContentResult<List<OrderReadModel>> okClosedOrdersResponse = (OkNegotiatedContentResult<List<OrderReadModel>>)closedOrdersResponse;
            List<OrderReadModel> closedOrders = okClosedOrdersResponse.Content;
            Assert.AreEqual(0, closedOrders[0].Price);
            Assert.AreEqual(10, closedOrders[0].Volume);
            Assert.AreEqual(0, closedOrders[0].VolumeExecuted);
            Assert.AreEqual(10, closedOrders[0].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[0].OrderType);
            Assert.AreEqual("Buy", closedOrders[0].OrderSide);
            Assert.AreEqual("Rejected", closedOrders[0].Status);

            Assert.AreEqual(252, closedOrders[1].Price);
            Assert.AreEqual(5, closedOrders[1].Volume);
            Assert.AreEqual(3, closedOrders[1].VolumeExecuted);
            Assert.AreEqual(2, closedOrders[1].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[1].OrderType);
            Assert.AreEqual("Sell", closedOrders[1].OrderSide);
            Assert.AreEqual("Cancelled", closedOrders[1].Status);

            Assert.AreEqual(250, closedOrders[2].Price);
            Assert.AreEqual(7, closedOrders[2].Volume);
            Assert.AreEqual(7, closedOrders[2].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[2].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[2].OrderType);
            Assert.AreEqual("Buy", closedOrders[2].OrderSide);
            Assert.AreEqual("Complete", closedOrders[2].Status);

            Assert.AreEqual(0, closedOrders[3].Price);
            Assert.AreEqual(3, closedOrders[3].Volume);
            Assert.AreEqual(3, closedOrders[3].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[3].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[3].OrderType);
            Assert.AreEqual("Buy", closedOrders[3].OrderSide);
            Assert.AreEqual("Complete", closedOrders[3].Status);

            Assert.AreEqual(0, closedOrders[4].Price);
            Assert.AreEqual(10, closedOrders[4].Volume);
            Assert.AreEqual(0, closedOrders[4].VolumeExecuted);
            Assert.AreEqual(10, closedOrders[4].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[4].OrderType);
            Assert.AreEqual("Buy", closedOrders[4].OrderSide);
            Assert.AreEqual("Rejected", closedOrders[4].Status);

            Assert.AreEqual(0, closedOrders[5].Price);
            Assert.AreEqual(5, closedOrders[5].Volume);
            Assert.AreEqual(5, closedOrders[5].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[5].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[5].OrderType);
            Assert.AreEqual("Sell", closedOrders[5].OrderSide);
            Assert.AreEqual("Complete", closedOrders[5].Status);

            Assert.AreEqual(0, closedOrders[6].Price);
            Assert.AreEqual(4, closedOrders[6].Volume);
            Assert.AreEqual(4, closedOrders[6].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[6].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[6].OrderType);
            Assert.AreEqual("Sell", closedOrders[6].OrderSide);
            Assert.AreEqual("Complete", closedOrders[6].Status);

            Assert.AreEqual(0, closedOrders[7].Price);
            Assert.AreEqual(5, closedOrders[7].Volume);
            Assert.AreEqual(5, closedOrders[7].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[7].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[7].OrderType);
            Assert.AreEqual("Sell", closedOrders[7].OrderSide);
            Assert.AreEqual("Complete", closedOrders[7].Status);
            //------------------------------------------------------------------------

            //------------------- Trades -------------------------
            IHttpActionResult tradesResponse = GetTrades(tradeController);
            OkNegotiatedContentResult<object> okTradeResponse = (OkNegotiatedContentResult<object>)tradesResponse;
            IList<object> tradesintermediateList = (IList<object>)okTradeResponse.Content;
            IList<object[]> trades = new List<object[]>();

            for (int i = 0; i < tradesintermediateList.Count; i++)
            {
                object[] objects = tradesintermediateList[i] as object[];
                trades.Add(objects);
            }

            // This call return list of object, so we have to explicitly check values within elements
            Assert.AreEqual(252, trades[0][2]);// Price
            Assert.AreEqual(3, trades[0][3]);// Volume
            Assert.AreEqual("XBTUSD", trades[0][4]);

            Assert.AreEqual(250, trades[1][2]);
            Assert.AreEqual(5, trades[1][3]);
            Assert.AreEqual("XBTUSD", trades[1][4]);

            Assert.AreEqual(250, trades[2][2]);
            Assert.AreEqual(2, trades[2][3]);
            Assert.AreEqual("XBTUSD", trades[2][4]);

            Assert.AreEqual(245, trades[3][2]);
            Assert.AreEqual(2, trades[3][3]);
            Assert.AreEqual("XBTUSD", trades[3][4]);

            Assert.AreEqual(245, trades[4][2]);
            Assert.AreEqual(5, trades[4][3]);
            Assert.AreEqual("XBTUSD", trades[4][4]);
            //-----------------------------------------------------------------------
        }


        [Test]
        [Category("Integration")]
        public void Scenario3Test_TestsScenario3AndItsOutcome_VerifiesThroughMarketDataOrderAndTradesResults()
        {
            // Get the context
            IApplicationContext applicationContext = ContextRegistry.GetContext();
            //Exchange exchange = new Exchange();
            //IEventStore inputEventStore = new RavenNEventStore(Constants.INPUT_EVENT_STORE);
            //IEventStore outputEventStore = new RavenNEventStore(Constants.OUTPUT_EVENT_STORE);
            //Journaler inputJournaler = new Journaler(inputEventStore);
            //Journaler outputJournaler = new Journaler(outputEventStore);
            //InputDisruptorPublisher.InitializeDisruptor(new IEventHandler<InputPayload>[] { exchange, inputJournaler });
            //OutputDisruptor.InitializeDisruptor(new IEventHandler<byte[]>[] { outputJournaler });

            // Get the instance through Spring configuration
            OrderController orderController = (OrderController)applicationContext["OrderController"];
            TradeController tradeController = (TradeController)applicationContext["TradeController"];
            MarketController marketController = (MarketController)applicationContext["MarketController"];
            orderController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            orderController.Request.Headers.Add("Auth", "123456789");
            tradeController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            tradeController.Request.Headers.Add("Auth", "123456789");
            marketController.Request = new HttpRequestMessage(HttpMethod.Post, "");
            marketController.Request.Headers.Add("Auth", "123456789");
            Scenario3OrderCreation(orderController);

            // ------------------------------- Order Book ------------------------------
            IHttpActionResult orderBookResponse = marketController.GetOrderBook("XBTUSD");

            OkNegotiatedContentResult<Tuple<OrderRepresentationList, OrderRepresentationList>> okOrderBookResponse =
                (OkNegotiatedContentResult<Tuple<OrderRepresentationList, OrderRepresentationList>>)orderBookResponse;

            Tuple<OrderRepresentationList, OrderRepresentationList> orderBook = okOrderBookResponse.Content;
            // Item1 = Bid Book, Item1[i].Item1 = Volume of 'i' Bid, Item1[i].Item2 = Price of 'i' Bid
            Assert.AreEqual(0, orderBook.Item1.Count());
            // Item2 = Bid Book, Item2[i].Item1 = Volume of Ask at index 'i', Item2[i].Item2 = Price of Bid at index 'i'
            Assert.AreEqual(0, orderBook.Item2.Count());
            // ------------------------------------------------------------------------

            // --------------------------------- Depth ---------------------------------
            IHttpActionResult depthResponse = marketController.GetDepth("XBTUSD");

            OkNegotiatedContentResult<Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]>> okDepth
                = (OkNegotiatedContentResult<Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]>>)depthResponse;

            Tuple<Tuple<decimal, decimal, int>[], Tuple<decimal, decimal, int>[]> depth = okDepth.Content;

            // Item1 = Bid Book, Item1.Item1 = Aggregated Volume, Item1.Item2 = Price, Item1.Item3 = Number of Orders
            Assert.IsNull(depth.Item1[0]);
            Assert.IsNull(depth.Item1[1]);
            Assert.IsNull(depth.Item1[2]);
            Assert.IsNull(depth.Item1[3]);
            Assert.IsNull(depth.Item1[4]);

            // Item2 = Bid Book, Item2.Item1 = Aggregated Volume, Item2.Item2 = Price, Item2.Item3 = Number of Orders
            Assert.IsNull(depth.Item2[0]);
            Assert.IsNull(depth.Item2[1]);
            Assert.IsNull(depth.Item2[2]);
            Assert.IsNull(depth.Item2[3]);
            Assert.IsNull(depth.Item2[4]);

            // -----------------------------------------------------------------------

            //------------------------- Open Orders ----------------------------------
            IHttpActionResult openOrdersResponse = GetOpenOrders(orderController);
            OkNegotiatedContentResult<List<OrderReadModel>> okOpenOrdersResponse = (OkNegotiatedContentResult<List<OrderReadModel>>)openOrdersResponse;
            List<OrderReadModel> openOrders = okOpenOrdersResponse.Content;

            Assert.AreEqual(0, openOrders.Count);
            //---------------------------------------------------------------------

            //-------------------------- Closed Orders ----------------------------
            IHttpActionResult closedOrdersResponse = GetClosedOrders(orderController);
            OkNegotiatedContentResult<List<OrderReadModel>> okClosedOrdersResponse = (OkNegotiatedContentResult<List<OrderReadModel>>)closedOrdersResponse;
            List<OrderReadModel> closedOrders = okClosedOrdersResponse.Content;

            Assert.AreEqual(252, closedOrders[0].Price);
            Assert.AreEqual(5, closedOrders[0].Volume);
            Assert.AreEqual(0, closedOrders[0].VolumeExecuted);
            Assert.AreEqual(5, closedOrders[0].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[0].OrderType);
            Assert.AreEqual("Sell", closedOrders[0].OrderSide);
            Assert.AreEqual("Cancelled", closedOrders[0].Status);

            Assert.AreEqual(245, closedOrders[1].Price);
            Assert.AreEqual(8, closedOrders[1].Volume);
            Assert.AreEqual(0, closedOrders[1].VolumeExecuted);
            Assert.AreEqual(8, closedOrders[1].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[1].OrderType);
            Assert.AreEqual("Buy", closedOrders[1].OrderSide);
            Assert.AreEqual("Cancelled", closedOrders[1].Status);

            Assert.AreEqual(250, closedOrders[2].Price);
            Assert.AreEqual(7, closedOrders[2].Volume);
            Assert.AreEqual(7, closedOrders[2].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[2].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[2].OrderType);
            Assert.AreEqual("Buy", closedOrders[2].OrderSide);
            Assert.AreEqual("Complete", closedOrders[2].Status);

            Assert.AreEqual(250, closedOrders[3].Price);
            Assert.AreEqual(3, closedOrders[3].Volume);
            Assert.AreEqual(2, closedOrders[3].VolumeExecuted);
            Assert.AreEqual(1, closedOrders[3].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[3].OrderType);
            Assert.AreEqual("Buy", closedOrders[3].OrderSide);
            Assert.AreEqual("Cancelled", closedOrders[3].Status);

            Assert.AreEqual(253, closedOrders[4].Price);
            Assert.AreEqual(5, closedOrders[4].Volume);
            Assert.AreEqual(0, closedOrders[4].VolumeExecuted);
            Assert.AreEqual(5, closedOrders[4].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[4].OrderType);
            Assert.AreEqual("Sell", closedOrders[4].OrderSide);
            Assert.AreEqual("Cancelled", closedOrders[4].Status);

            Assert.AreEqual(240, closedOrders[5].Price);
            Assert.AreEqual(8, closedOrders[5].Volume);
            Assert.AreEqual(0, closedOrders[5].VolumeExecuted);
            Assert.AreEqual(8, closedOrders[5].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[5].OrderType);
            Assert.AreEqual("Buy", closedOrders[5].OrderSide);
            Assert.AreEqual("Cancelled", closedOrders[5].Status);

            Assert.AreEqual(245, closedOrders[6].Price);
            Assert.AreEqual(7, closedOrders[6].Volume);
            Assert.AreEqual(0, closedOrders[6].VolumeExecuted);
            Assert.AreEqual(7, closedOrders[6].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[6].OrderType);
            Assert.AreEqual("Buy", closedOrders[6].OrderSide);
            Assert.AreEqual("Cancelled", closedOrders[6].Status);

            Assert.AreEqual(247, closedOrders[7].Price);
            Assert.AreEqual(3, closedOrders[7].Volume);
            Assert.AreEqual(0, closedOrders[7].VolumeExecuted);
            Assert.AreEqual(3, closedOrders[7].OpenQuantity);
            Assert.AreEqual("Limit", closedOrders[7].OrderType);
            Assert.AreEqual("Buy", closedOrders[7].OrderSide);
            Assert.AreEqual("Cancelled", closedOrders[7].Status);

            Assert.AreEqual(0, closedOrders[8].Price);
            Assert.AreEqual(9, closedOrders[8].Volume);
            Assert.AreEqual(9, closedOrders[8].VolumeExecuted);
            Assert.AreEqual(0, closedOrders[8].OpenQuantity);
            Assert.AreEqual("Market", closedOrders[8].OrderType);
            Assert.AreEqual("Sell", closedOrders[8].OrderSide);
            Assert.AreEqual("Complete", closedOrders[8].Status);
            //------------------------------------------------------------------------

            //------------------- Trades -------------------------

            IHttpActionResult tradesResponse = GetTrades(tradeController);
            OkNegotiatedContentResult<object> okTradeResponse = (OkNegotiatedContentResult<object>)tradesResponse;
            IList<object> tradesintermediateList = (IList<object>)okTradeResponse.Content;
            IList<object[]> trades = new List<object[]>();

            for (int i = 0; i < tradesintermediateList.Count; i++)
            {
                object[] objects = tradesintermediateList[i] as object[];
                trades.Add(objects);
            }

            // This call return list of object, so we have to explicitly check values within elements
            Assert.AreEqual(250, trades[0][2]);// Price
            Assert.AreEqual(7, trades[0][3]);// Volume
            Assert.AreEqual("XBTUSD", trades[0][4]);

            Assert.AreEqual(250, trades[1][2]);
            Assert.AreEqual(2, trades[1][3]);
            Assert.AreEqual("XBTUSD", trades[1][4]);
            //-----------------------------------------------------------------------
        }

        #region Client Methods

        /// <summary>
        /// Testing scenario 1
        /// </summary>
        private void Scenario1OrderCreation(OrderController orderController)
        {
            string currecyPair = "XBTUSD";
            //Create orders
            Console.WriteLine(orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 250, 10)));
            Thread.Sleep(2000);
            Console.WriteLine(orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "limit", 252, 5)));
            Thread.Sleep(2000);
            Console.WriteLine(orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "market", 0, 3)));
            Thread.Sleep(2000);
            Console.WriteLine(orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 253, 2)));
            Thread.Sleep(2000);
            Console.WriteLine(orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "market", 0, 5)));
            Thread.Sleep(2000);
            Console.WriteLine(orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 250, 2)));
            Thread.Sleep(4000);
        }

        /// <summary>
        /// Scenrio 2 order creation and sending
        /// </summary>
        /// <param name="orderController"></param>
        private void Scenario2OrderCreation(OrderController orderController)
        {
            string currecyPair = "XBTUSD";
            IHttpActionResult httpActionResult1 = (orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "market", 0, 10)));
            Thread.Sleep(2000);
            string orderId1 = this.GetOrderId(httpActionResult1);
            IHttpActionResult httpActionResult2 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "limit", 252, 5));
            string orderId2 = this.GetOrderId(httpActionResult2);
            Thread.Sleep(2000);
            IHttpActionResult httpActionResult3 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 245, 8));
            string orderId3 = this.GetOrderId(httpActionResult3);
            Thread.Sleep(2000);
            IHttpActionResult httpActionResult4 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 250, 7));
            string orderId4 = this.GetOrderId(httpActionResult4);
            Thread.Sleep(2000);
            orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "market", 0, 3));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId2));
            Thread.Sleep(2000);
            orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "market", 0, 10));
            Thread.Sleep(2000);
            orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "market", 0, 5));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId2));
            Thread.Sleep(2000);
            orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "market", 0, 4));
            Thread.Sleep(2000);
            orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "market", 0, 5));
            Thread.Sleep(4000);
        }

        /// <summary>
        /// Creates and sends order for Scenario 3
        /// </summary>
        /// <param name="orderController"></param>
        private void Scenario3OrderCreation(OrderController orderController)
        {
            string currecyPair = "XBTUSD";
            IHttpActionResult httpActionResult1 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "limit", 252, 5));
            Thread.Sleep(2000);
            string orderId1 = this.GetOrderId(httpActionResult1);
            IHttpActionResult httpActionResult2 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 245, 8));
            Thread.Sleep(2000);
            string orderId2 = this.GetOrderId(httpActionResult2);
            IHttpActionResult httpActionResult3 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 250, 7));
            Thread.Sleep(2000);
            string orderId3 = this.GetOrderId(httpActionResult3);
            IHttpActionResult httpActionResult4 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 250, 3));
            Thread.Sleep(2000);
            string orderId4 = this.GetOrderId(httpActionResult4);
            IHttpActionResult httpActionResult5 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "limit", 253, 5));
            Thread.Sleep(2000);
            string orderId5 = this.GetOrderId(httpActionResult5);
            IHttpActionResult httpActionResult6 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 240, 8));
            Thread.Sleep(2000);
            string orderId6 = this.GetOrderId(httpActionResult6);
            IHttpActionResult httpActionResult7 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 245, 7));
            Thread.Sleep(2000);
            string orderId7 = this.GetOrderId(httpActionResult7);
            IHttpActionResult httpActionResult8 = orderController.CreateOrder(new CreateOrderParam(currecyPair, "buy", "limit", 247, 3));
            Thread.Sleep(2000);
            string orderId8 = this.GetOrderId(httpActionResult8);
            System.Console.WriteLine(orderController.CancelOrder(orderId6));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId1));
            Thread.Sleep(2000);
            orderController.CreateOrder(new CreateOrderParam(currecyPair, "sell", "market", 0, 9));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId8));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId7));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId6));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId5));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId4));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId3));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId2));
            Thread.Sleep(2000);
            System.Console.WriteLine(orderController.CancelOrder(orderId1));
            Thread.Sleep(5000);
        }

        /// <summary>
        /// Extracts OrderID from the HttpActionresult
        /// </summary>
        /// <param name="httpActionResult"></param>
        /// <returns></returns>
        public string GetOrderId(IHttpActionResult httpActionResult)
        {
            OkNegotiatedContentResult<NewOrderRepresentation> okResponseMessage =
                (OkNegotiatedContentResult<NewOrderRepresentation>)httpActionResult;
            NewOrderRepresentation newOrderRepresentation = okResponseMessage.Content;
            return newOrderRepresentation.OrderId;
        }

        /// <summary>
        /// Get Open Orders
        /// </summary>
        private dynamic GetOpenOrders(OrderController orderController)
        {
            return orderController.QueryOpenOrders("true");
        }

        /// <summary>
        /// Get Closed Orders
        /// </summary>
        /// <param name="orderController"></param>
        /// <returns></returns>
        private dynamic GetClosedOrders(OrderController orderController)
        {
            return orderController.QueryClosedOrders(new QueryClosedOrdersParams(true, "", ""));
        }

        /// <summary>
        /// Get Trades
        /// </summary>
        /// <param name="tradeController"></param>
        /// <returns></returns>
        private dynamic GetTrades(TradeController tradeController)
        {
            return tradeController.GetTradeHistory(new TradeHistoryParams("", ""));
        }

        #endregion Client Methods
    }
}
