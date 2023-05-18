using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using NCI.OCPL.Api.Common;


namespace NCI.OCPL.Api.CTSListingPages.Controllers
{

    /// <summary>
    /// Controller for reporting on system health.
    /// </summary>
    [Route("HealthCheck")]
    public class HealthCheckController : ControllerBase
    {

        /// <summary>
        /// Message to return for a "healthy" status.
        /// </summary>
        public const string HEALTHY_STATUS = "alive!";

        /// <summary>
        /// Message to return for an "unhealthy" status.
        /// </summary>
        public const string UNHEALTHY_STATUS = "Service not healthy.";

        private readonly IListingInfoHealthService _listingInfoHealthSvc;
        private readonly ITrialTypeHealthService _trialTypeHealthSvc;

        private readonly ILogger _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="listingInfoHealthSvc">Elasticsearch health check instance for listing information.</param>
        /// <param name="trialTypeHealthSvc">Elasticsearch health check instance for trial type information.</param>
        /// <returns></returns>
        public HealthCheckController(ILogger<HealthCheckController> logger,
                IListingInfoHealthService listingInfoHealthSvc,
                ITrialTypeHealthService trialTypeHealthSvc)
            => (_logger, _listingInfoHealthSvc, _trialTypeHealthSvc) = (logger, listingInfoHealthSvc, trialTypeHealthSvc);


        /// <summary>
        /// Provides an endpoint for checking that the API and its underlying Elasticsearch instance
        /// are in a good operational state.
        /// </summary>
        /// <returns>Status 200 and the message "alive!" if the service is healthy.
        /// Status 500  and the message "Service not healthy." otherwise.</returns>
        [HttpGet("status")]
        public async Task<string> IsHealthy()
        {
            try
            {
                IEnumerable<bool> isHealthy = await Task.WhenAll(
                    _listingInfoHealthSvc.IndexIsHealthy(),
                    _trialTypeHealthSvc.IndexIsHealthy()
                );

                if (isHealthy.All( healthy => healthy))
                    return HEALTHY_STATUS;
                else
                    throw new APIErrorException(500, UNHEALTHY_STATUS);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health.");
                throw new APIErrorException(500, UNHEALTHY_STATUS);
            }
        }
    }
}