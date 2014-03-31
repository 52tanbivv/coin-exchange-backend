﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using CoinExchange.Trades.Application.MarketData;

namespace CoinExchange.Trades.Port.Adapter.Rest.Resources
{
    /// <summary>
    /// Market Data Service class rest expose
    /// </summary>
    public class MarketResource : ApiController
    {
        private MarketDataApplicationService _marketDataService = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MarketResource()
        {
            _marketDataService = new MarketDataApplicationService();
        }

        [HttpGet]
        [Route("marketData/tickerinfo")]
        public IHttpActionResult TickerInfo(string pair)
        {
            try
            {
                return Ok(_marketDataService.GetTickerInfo(pair));

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        [HttpGet]
        [Route("marketData/ohlcinfo")]
        public IHttpActionResult OhlcInfo(string pair, int interval = 1, string since = "")
        {
            try
            {
                return Ok(_marketDataService.GetOhlcInfo(pair,interval, since));

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        /// <summary>
        /// Returns orders that have not been executed but those that have been accepted on the server. Exception can be 
        /// provided in the second parameter
        /// Params:
        /// 1. TraderId(ValueObject)[FromBody]: Contains an Id of the trader, used for authentication of the trader
        /// 2. includeTrades(bool): Include trades as well in the response(optional)
        /// 3. userRefId: Restrict results to given user reference id (optional)
        /// </summary>
        /// <returns></returns>
        [Route("marketdata/orderbook")]
        [HttpGet]
        public IHttpActionResult OpenOrderList(string currencyPair, int count = 0)
        {
            try
            {
                List<object> list = _marketDataService.GetOrderBook(currencyPair);
                if (list != null)
                {
                    return Ok<List<object>>(list);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
