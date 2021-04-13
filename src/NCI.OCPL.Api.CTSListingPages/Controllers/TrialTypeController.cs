using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages;

namespace NCI.OCPL.Api.CTSListingPages.Controllers
{
    /// <summary>
    /// Controller for routes used when searching for or retrieving trial type data.
    /// </summary>
    [Route("trial-type")]
    public class TrialTypeController : ControllerBase
    {
        /// <summary>
        /// The logger instance.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The query service to use.
        /// </summary>
        private readonly ITrialTypeQueryService _trialTypeQueryService;

        /// <summary>
        /// Constructor
        /// </summary>
        public TrialTypeController(ILogger<TrialTypeController> logger, ITrialTypeQueryService service)
        {
            _logger = logger;
            _trialTypeQueryService = service;
        }

        /// <summary>
        /// Retrieve a single TrialTypeInfo with a pretty-url name or identifier exactly matching the name parameter.
        /// </summary>
        /// <param name="name">The name - either the pretty-url name or identifier string - of the record to be retrieved.</param>
        /// <returns>A TrialTypeInfo object or status 404 if an exact match is not found.</returns>
        [HttpGet("{name}")]
        public async Task<TrialTypeInfo> Get(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new APIErrorException(400, "You must specify the name parameter.");


            TrialTypeInfo result;
            try
            {
                result = await _trialTypeQueryService.Get(name);
            }
            catch (APIInternalException)
            {
                throw new APIErrorException(500, "Errors occured.");
            }

            if (result == null)
                throw new APIErrorException(404, "Could not find the requested label or identifier.");

            return result;
        }
    }
}
