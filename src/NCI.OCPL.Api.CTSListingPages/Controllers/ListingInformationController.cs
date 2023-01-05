using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        /// Missing prettyURlName parameter.
        /// </summary>
        public const string MISSING_PRETTY_URL_NAME_MESSAGE = "You must specify the prettyUrlName parameter.";

        /// <summary>
        /// prettyURlName not found.
        /// </summary>
        public const string PRETTY_URL_NAME_NOT_FOUND_MESSAGE = "Could not find the requested pretty URL name.";

        /// <summary>
        /// Invalid characters in pretty URL name.
        /// </summary>
        public const string PRETTY_URL_INVALID_MESSAGE = "Pretty URL name has invalid format.";

        /// <summary>
        /// Missing ccode parameter.
        /// </summary>
        public const string MISSING_CCODE_MESSAGE = "You must specify at least one ccode parameter.";

        /// <summary>
        /// Invalid ccode parameter.
        /// </summary>
        public const string INVALID_CCODE_MESSAGE = "One or more invalid ccode values found.";

        /// <summary>
        /// ccode not found.
        /// </summary>
        public const string CCODE_NOT_FOUND_MESSAGE = "Could not find the requested codes.";

        /// <summary>
        /// Multiple results for ccodes.
        /// </summary>
        public const string CCODE_MULTIPLE_RESULT_MESSAGE = "Multiple records found.";

        /// <summary>
        /// Generic internal error.
        /// </summary>
        public const string INTERNAL_ERROR_MESSAGE = "Errors occured.";

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
            Regex PrettyUrlValidator = new Regex("^[a-zA-Z0-9]+[a-zA-Z0-9\\-]*$", RegexOptions.IgnoreCase);

            if (String.IsNullOrWhiteSpace(prettyUrlName))
                throw new APIErrorException(400, MISSING_PRETTY_URL_NAME_MESSAGE);

            if(!PrettyUrlValidator.IsMatch(prettyUrlName))
                throw new APIErrorException(400, PRETTY_URL_INVALID_MESSAGE);

            ListingInfo result;
            try
            {
                result = await _listingInfoQueryService.GetByPrettyUrlName(prettyUrlName);
            }
            catch (APIInternalException)
            {
                throw new APIErrorException(500, INTERNAL_ERROR_MESSAGE);
            }

            if (result == null)
                throw new APIErrorException(404, PRETTY_URL_NAME_NOT_FOUND_MESSAGE);

            return result;
        }

        /// <summary>
        /// Retrieve the listing information for concept record(s) by exactly or partially matching
        /// the c-code (list) parameter.
        /// </summary>
        /// <param name="ccode">The c-code list of the record(s) to be retrieved.</param>
        /// <returns>A ListingInfo object containing the concept data, or status 404 if exact or partial matches are not found.</returns>
        [HttpGet("get")]
        public async Task<ListingInfo> GetByIds([FromQuery] string[] ccode)
        {
            Regex CCodeValidator = new Regex("^C[0-9]+$", RegexOptions.IgnoreCase);

            // If the array is null, empty, or contains empty strings, throw an error.
            if (ccode == null || ccode.Length == 0 || ccode.Any(c => string.IsNullOrWhiteSpace(c)))
                throw new APIErrorException(400, MISSING_CCODE_MESSAGE);

            // Now that we know there are no nulls, trim all the strings.
            ccode = ccode.Select(c => c.Trim()).ToArray();

            if( ccode.Any(c => !CCodeValidator.IsMatch(c)))
                throw new APIErrorException(400, INVALID_CCODE_MESSAGE);

            ListingInfo[] results;
            try
            {
                results = await _listingInfoQueryService.GetByIds(ccode);
            }
            catch (APIInternalException)
            {
                throw new APIErrorException(500, INTERNAL_ERROR_MESSAGE);
            }

            if (results == null || results.Length == 0)
                throw new APIErrorException(404, CCODE_NOT_FOUND_MESSAGE);

            if (results.Length > 1)
            {
                _logger.LogWarning($"Multiple records found for code(s): '{String.Join(',', ccode)}'.");
                throw new APIErrorException(409, CCODE_MULTIPLE_RESULT_MESSAGE);
            }

            return results.First();
        }
    }
}