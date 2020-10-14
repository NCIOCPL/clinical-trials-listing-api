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
    /// Controller for routes used when searching for or retrieving concept data for clinical trial
    /// listing pages.
    /// </summary>
    [Route("listing-information")]
    [ApiController]
    public class ListingInformationController : ControllerBase
    {
        /// <summary>
        /// The logger instance.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The query service to use.
        /// </summary>
        private readonly IListingInfoQueryService _listingInfoQueryService;

        /// <summary>
        /// Constructor
        /// </summary>
        public ListingInformationController(ILogger<ListingInformationController> logger, IListingInfoQueryService service)
        {
            _logger = logger;
            _listingInfoQueryService = service;
        }

        /// <summary>
        /// Retrieve the listing information for a single concept record by exactly matching the
        /// its pretty-url name parameter.
        /// </summary>
        /// <param name="prettyUrlName">The pretty-url name of the record to be retrieved.</param>
        /// <returns>A ListingInfo object containing the concept data or status 404 if an exact match is not found.</returns>
        [HttpGet("{prettyUrlName}")]
        public async Task<ListingInfo> GetByPrettyUrlName(string prettyUrlName)
        {
            if (String.IsNullOrWhiteSpace(prettyUrlName))
                throw new APIErrorException(400, "You must specify the prettyUrlName parameter.");


            ListingInfo result;
            try
            {
                result = await _listingInfoQueryService.GetByPrettyUrlName(prettyUrlName);
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