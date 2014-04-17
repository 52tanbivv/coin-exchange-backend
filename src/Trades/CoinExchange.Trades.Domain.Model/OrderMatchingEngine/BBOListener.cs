﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinExchange.Trades.Domain.Model.OrderMatchingEngine
{
    /// <summary>
    /// Listens to the best bid and best ask on the Order Book
    /// </summary>
    public class BBOListener : IBBOListener
    {
        // Get the Current Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Implementation of IBBOListener

        /// <summary>
        /// 
        /// </summary>
        /// <param name="askDepthLevel"></param>
        /// <param name="bidDepthLevel"></param>
        public void OnBBOChange(DepthLevel askDepthLevel, DepthLevel bidDepthLevel)
        {
            Log.Debug("Best bid and offer received.");
        }

        #endregion
    }
}
