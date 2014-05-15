﻿using System.Configuration;
using System.Threading;
using CoinExchange.Common.Tests;
using CoinExchange.Trades.Domain.Model.OrderAggregate;
using CoinExchange.Trades.Domain.Model.Services;
using CoinExchange.Trades.Domain.Model.TradeAggregate;
using CoinExchange.Trades.Infrastructure.Persistence.RavenDb;
using CoinExchange.Trades.Infrastructure.Services;
using CoinExchange.Trades.ReadModel.DTO;
using CoinExchange.Trades.ReadModel.EventHandlers;
using CoinExchange.Trades.ReadModel.Repositories;
using Disruptor;
using NHibernate;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Spring.Context.Support;
using Constants = CoinExchange.Common.Domain.Model.Constants;

namespace CoinExchange.Trades.ReadModel.Persistence.Tests
{
    [TestFixture]
    public class TradeListenerTests
    {
        private ManualResetEvent _manualResetEvent;
        private IEventStore _eventStore;
        private TradeEventListener _listener = ContextRegistry.GetContext()["TradeEventListener"] as TradeEventListener;
        private IPersistanceRepository _persistance = ContextRegistry.GetContext()["PersistenceRepository"] as IPersistanceRepository;
        private ITradeRepository _tradeRepository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
        private DatabaseUtility _databaseUtility;

        [SetUp]
        public new void SetUp()
        {
            BeforeSetup();
            var connection = ConfigurationManager.ConnectionStrings["MySql"].ToString();
            _databaseUtility = new DatabaseUtility(connection);
            _databaseUtility.Create();
            _databaseUtility.Populate();
            //_persistance = ContextRegistry.GetContext()["PersistenceRepository"] as IPersistanceRepository;
            //_orderRepository = ContextRegistry.GetContext()["OrderRepository"] as IOrderRepository;
            //initialize journaler
            _eventStore = new RavenNEventStore(Constants.OUTPUT_EVENT_STORE);
            Journaler journaler = new Journaler(_eventStore);
            //assign journaler to disruptor as its consumer
            OutputDisruptor.InitializeDisruptor(new IEventHandler<byte[]>[] { journaler });
            _manualResetEvent = new ManualResetEvent(false);
          //  _listener = new TradeEventListener(_persistance);
            AfterSetup();
        }

        [Test]
        [Category("Integration")]
        public void PublishTradeToOutputDisruptor_IfTradeListenerIsInitiated_ItShouldSaveInDatabase()
        {
            Order buyOrder = OrderFactory.CreateOrder("123", "XBTUSD", "limit", "buy", 10, 100,
              new StubbedOrderIdGenerator());
            Order sellOrder = OrderFactory.CreateOrder("1234", "XBTUSD", "limit", "sell", 10, 100,
               new StubbedOrderIdGenerator());
            //Trade trade=new Trade("XBTUSD",new Price(100),new Volume(10),DateTime.Now,buyOrder,sellOrder);
            Trade trade = TradeFactory.GenerateTrade("XBTUSD", new Price(1000), new Volume(10), buyOrder, sellOrder);
            OutputDisruptor.Publish(trade);
            _manualResetEvent.WaitOne(5000);
            TradeReadModel model = _tradeRepository.GetById(trade.TradeId.Id.ToString());
            Assert.NotNull(model);
            Assert.AreEqual(model.BuyOrderId,buyOrder.OrderId.Id.ToString());
            Assert.AreEqual(model.SellOrderId, sellOrder.OrderId.Id.ToString());
            Assert.AreEqual(model.Price,1000);
            Assert.AreEqual(model.CurrencyPair,"XBTUSD");
            Assert.AreEqual(model.BuyTraderId,"123");
            Assert.AreEqual(model.SellTraderId, "1234");
            Assert.AreEqual(model.Volume,10);
        }

        [TearDown]
        public new void TearDown()
        {
            BeforeTearDown();
            _databaseUtility.Create();
            OutputDisruptor.ShutDown();
            AfterTearDown();
        }

        protected virtual void BeforeSetup() { }
        protected virtual void AfterSetup() { }
        protected virtual void BeforeTearDown() { }
        protected virtual void AfterTearDown() { }
    }
}
