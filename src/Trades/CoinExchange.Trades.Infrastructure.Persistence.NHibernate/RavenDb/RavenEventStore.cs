﻿using System;
using CoinExchange.Trades.Domain.Model.Services;
using Raven.Client;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace CoinExchange.Trades.Infrastructure.Persistence.RavenDb
{
    /// <summary>
    /// Ravendb implementation of eventstore
    /// </summary>
    public class RavenEventStore:IEventStore
    {
        private IDocumentStore _documentStore;
        public RavenEventStore()
        {
            //TODO: need to add port on some config file
            _documentStore = new DocumentStore { Url = "http://localhost:8081" }.Initialize();
        }
        public bool StoreEvent(object id,string eventName, object blob)
        {
            object obj = new {Aggregate = id, EventName = eventName, Details = blob};
                _documentStore.DatabaseCommands.Put("events/", null, RavenJObject.FromObject(obj), new RavenJObject());
            return true;
        }
    }
}
