﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinExchange.Trades.Domain.Model.OrderAggregate;
using Disruptor;

namespace CoinExchange.Trades.Domain.Model.Services
{
    /// <summary>
    /// Journaler for saving events
    /// </summary>
    public class Journaler:IEventHandler<InputPayload>
    {
        private IEventStore _eventStore;
        private InputPayload _receivedPayload;
        public Journaler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public void OnNext(InputPayload data, long sequence, bool endOfBatch)
        {
            _receivedPayload = new InputPayload() { OrderCancellation = new OrderCancellation(), Order = new Order() };
            if (data.IsOrder)
            {
                data.Order.MemberWiseClone(_receivedPayload.Order);
                _receivedPayload.IsOrder = true;
                _eventStore.StoreEvent(_receivedPayload.Order);
            }
            else
            {
                data.OrderCancellation.MemberWiseClone(_receivedPayload.OrderCancellation);
                _receivedPayload.IsOrder = false;
                _eventStore.StoreEvent(_receivedPayload.OrderCancellation);
            }
        }
    }
}
