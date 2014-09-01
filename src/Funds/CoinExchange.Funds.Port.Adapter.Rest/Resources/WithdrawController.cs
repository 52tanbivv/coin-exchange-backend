﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CoinExchange.Common.Services;
using CoinExchange.Funds.Application.WithdrawServices;
using CoinExchange.Funds.Application.WithdrawServices.Commands;
using CoinExchange.Funds.Port.Adapter.Rest.DTOs.Withdraw;

namespace CoinExchange.Funds.Port.Adapter.Rest.Resources
{
    /// <summary>
    /// Withdraw Controller
    /// </summary>
    [RoutePrefix("v1")]
    public class WithdrawController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IWithdrawApplicationService _withdrawApplicationService;
        private IApiKeyInfoAccess _apiKeyInfoAccess;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="withdrawApplicationService"></param>
        /// <param name="apiKeyInfoAccess"></param>
        public WithdrawController(IWithdrawApplicationService withdrawApplicationService, IApiKeyInfoAccess apiKeyInfoAccess)
        {
            _withdrawApplicationService = withdrawApplicationService;
            _apiKeyInfoAccess = apiKeyInfoAccess;
        }

        [Route("funds/getrecentwithdrawals")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult GetRecentWithdrawals([FromBody]GetRecentWithdrawalsParams recentWithdrawalsParams)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Get Recent Withdrawals call: Currency = {0}", recentWithdrawalsParams.Currency));
            }
            try
            {
                //get api key from header
                var headers = Request.Headers;
                string apikey = "";
                IEnumerable<string> headerParams;
                if (headers.TryGetValues("Auth", out headerParams))
                {
                    string[] auth = headerParams.ToList()[0].Split(',');
                    apikey = auth[0];
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Get Recent Withdrawals Call: ApiKey = {0}", apikey));
                }
                if (recentWithdrawalsParams != null && !string.IsNullOrEmpty(recentWithdrawalsParams.Currency))
                {
                    int accountId = _apiKeyInfoAccess.GetUserIdFromApiKey(apikey);
                    return Ok(_withdrawApplicationService.GetRecentWithdrawals(accountId, recentWithdrawalsParams.Currency));
                }
                return BadRequest("Currency is not provided.");
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format("Get Recent Withdrawals Call Error: {0}", exception));
                }
                return InternalServerError();
            }
        }

        /// <summary>
        /// Call to add a new withdraw address
        /// </summary>
        /// <param name="addWithdrawAddress"> </param>
        /// <returns></returns>
        [Route("funds/addwithdrawaddress")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult AddAddress([FromBody]AddWithdrawAddressParams addWithdrawAddress)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Withdraw Address Save call: Currency = {0}", addWithdrawAddress.Currency));
            }
            try
            {
                //get api key from header
                var headers = Request.Headers;
                string apikey = "";
                IEnumerable<string> headerParams;
                if (headers.TryGetValues("Auth", out headerParams))
                {
                    string[] auth = headerParams.ToList()[0].Split(',');
                    apikey = auth[0];
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Withdraw Address Save Call: ApiKey = {0}", apikey));
                }
                if (addWithdrawAddress != null && addWithdrawAddress.Currency != string.Empty)
                {
                    int accountId = _apiKeyInfoAccess.GetUserIdFromApiKey(apikey);
                    return Ok(_withdrawApplicationService.AddAddress(new AddAddressCommand(accountId, 
                        addWithdrawAddress.Currency, addWithdrawAddress.BitcoinAddress, addWithdrawAddress.Description)));
                }
                return BadRequest("Currency is not provided.");
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format("Withdraw Address Save Call Error: {0}", exception));
                }
                return InternalServerError();
            }
        }

        /// <summary>
        /// Call to get all the withdrawal addresses  of the given currency for this user
        /// </summary>
        /// <param name="getWithdrawAddressesParams"> </param>
        /// <returns></returns>
        [Route("funds/getwithdrawaddresses")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult GetWithdrawAddresses([FromBody]GetWithdrawAddressesParams getWithdrawAddressesParams)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Get Withdraw Addresses call: Currency = {0}", getWithdrawAddressesParams.Currency));
            }
            try
            {
                //get api key from header
                var headers = Request.Headers;
                string apikey = "";
                IEnumerable<string> headerParams;
                if (headers.TryGetValues("Auth", out headerParams))
                {
                    string[] auth = headerParams.ToList()[0].Split(',');
                    apikey = auth[0];
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Get Withdraw Addresses Call: ApiKey = {0}", apikey));
                }
                if (!string.IsNullOrEmpty(getWithdrawAddressesParams.Currency))
                {
                    int accountId = _apiKeyInfoAccess.GetUserIdFromApiKey(apikey);
                    return Ok(_withdrawApplicationService.GetWithdrawalAddresses(accountId));
                }
                return BadRequest("Currency is not provided.");
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format("Get Withdraw Addresses Call Error: {0}", exception));
                }
                return InternalServerError();
            }
        }

        /// <summary>
        /// Call to commit a new Withdraw for the given currency for this user
        /// </summary>
        /// <param name="commitWithdrawParams"> </param>
        /// <returns></returns>
        [Route("funds/commitwithdraw")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult CommitWithdraw([FromBody]CommitWithdrawParams commitWithdrawParams)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Commit Withdraw call: Currency = {0}", commitWithdrawParams.Currency));
            }
            try
            {
                //get api key from header
                var headers = Request.Headers;
                string apikey = "";
                IEnumerable<string> headerParams;
                if (headers.TryGetValues("Auth", out headerParams))
                {
                    string[] auth = headerParams.ToList()[0].Split(',');
                    apikey = auth[0];
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Commit Withdraw Call: ApiKey = {0}", apikey));
                }
                if (!string.IsNullOrEmpty(commitWithdrawParams.Currency))
                {
                    int accountId = _apiKeyInfoAccess.GetUserIdFromApiKey(apikey);
                    return Ok(_withdrawApplicationService.CommitWithdrawal(new CommitWithdrawCommand(accountId,
                        commitWithdrawParams.Currency, commitWithdrawParams.IsCryptoCurrency, commitWithdrawParams.BitcoinAddress,
                        commitWithdrawParams.Amount)));
                }
                return BadRequest("Currency is not provided.");
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format("Commit Withdraw Call Error: {0}", exception));
                }
                return InternalServerError();
            }
        }

        /// <summary>
        /// Call to get all the withdrawal limits and funds information for this user
        /// </summary>
        /// <param name="getWithdrawLimitsParams"> </param>
        /// <returns></returns>
        [Route("funds/getwithdrawlimits")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult GetWithdrawLimits([FromBody]GetWithdrawLimitsParams getWithdrawLimitsParams)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Get Withdrawal Limits call: Currency = {0}", getWithdrawLimitsParams.Currency));
            }
            try
            {
                //get api key from header
                var headers = Request.Headers;
                string apikey = "";
                IEnumerable<string> headerParams;
                if (headers.TryGetValues("Auth", out headerParams))
                {
                    string[] auth = headerParams.ToList()[0].Split(',');
                    apikey = auth[0];
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Get Withdrawal Limits Call: ApiKey = {0}", apikey));
                }
                if (!string.IsNullOrEmpty(getWithdrawLimitsParams.Currency))
                {
                    int accountId = _apiKeyInfoAccess.GetUserIdFromApiKey(apikey);
                    return Ok(_withdrawApplicationService.GetWithdrawLimitThresholds(accountId, getWithdrawLimitsParams.Currency));
                }
                return BadRequest("Currency is not provided.");
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format("Get Withdrawal Limits Call Error: {0}", exception));
                }
                return InternalServerError();
            }
        }

        [Route("funds/deletewithdrawaddress")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult DeleteWithdrawAddress([FromBody]DeleteWithdrawAddressParams deleteWithdrawAddressParams)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Delete Withdraw Address call."));
            }
            try
            {
                //get api key from header
                var headers = Request.Headers;
                string apikey = "";
                IEnumerable<string> headerParams;
                if (headers.TryGetValues("Auth", out headerParams))
                {
                    string[] auth = headerParams.ToList()[0].Split(',');
                    apikey = auth[0];
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Delete Withdraw Address Call: ApiKey = {0}", apikey));
                }
                if (!string.IsNullOrEmpty(apikey))
                {
                    int accountId = _apiKeyInfoAccess.GetUserIdFromApiKey(apikey);
                    return Ok(_withdrawApplicationService.DeleteAddress(new DeleteWithdrawAddressCommand(
                        accountId, deleteWithdrawAddressParams.BitcoinAddress)));
                }
                return BadRequest("Currency is not provided.");
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format("Delete Withdraw Address Call Error: {0}", exception));
                }
                return InternalServerError();
            }
        }

        [Route("funds/cancelWithdraw")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult CancelWithdraw([FromBody]CancelWithdrawParams cancelWithdrawParams)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("Cancel Withdraw call."));
            }
            try
            {
                //get api key from header
                var headers = Request.Headers;
                string apikey = "";
                IEnumerable<string> headerParams;
                if (headers.TryGetValues("Auth", out headerParams))
                {
                    string[] auth = headerParams.ToList()[0].Split(',');
                    apikey = auth[0];
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Cancel Withdraw Call: ApiKey = {0}", apikey));
                }
                if (!string.IsNullOrEmpty(apikey))
                {
                    return Ok(_withdrawApplicationService.CancelWithdraw(new CancelWithdrawCommand(cancelWithdrawParams.WithdrawId)));
                }
                return BadRequest("Currency is not provided.");
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format("Cancel Withdraw Call Error: {0}", exception));
                }
                return InternalServerError();
            }
        }
    }
}
