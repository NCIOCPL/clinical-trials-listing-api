using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages;

namespace NCI.OCPL.Api.CTSListingPages.Controllers
{
    /// <summary>
    /// Controller for routes used when searching for or retrieving EVS override naming data.
    /// </summary>
    [Route("override-name")]
    [ApiController]
    public class OverrideController : ControllerBase
    {
        /// <summary>
        /// The logger instance.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The query service to use.
        /// </summary>
        private readonly IOverridesQueryService _overridesQueryService;

        /// <summary>
        /// Constructor
        /// </summary>
        public OverrideController(ILogger<OverrideController> logger, IOverridesQueryService service)
        {
            _logger = logger;
            _overridesQueryService = service;
        }

        /// <summary>
        /// Retrieve a single OverrideRecord with a pretty-url name exactly matching the name parameter.
        /// </summary>
        /// <param name="prettyUrlName">The pretty-url name of the record to be retrieved.</param>
        /// <returns>An OverrideRecord object or status 404 if an exact match is not found.</returns>
        [HttpGet("{prettyUrlName}")]
        public async Task<OverrideRecord> GetByPrettyUrlName(string prettyUrlName)
        {
            if (String.IsNullOrWhiteSpace(prettyUrlName))
                throw new APIErrorException(400, "You must specify the prettyUrlName parameter.");


            OverrideRecord result;
            try
            {
                result = await _overridesQueryService.GetByPrettyUrlName(prettyUrlName);
            }
            catch (APIInternalException)
            {
                throw new APIErrorException(500, "Errors occured.");
            }

            if (result == null)
                throw new APIErrorException(404, $"Could not find pretty URL name '{prettyUrlName}'.");

            return result;
        }
    }
}