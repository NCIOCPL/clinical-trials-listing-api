using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        /// Missing name parameter.
        /// </summary>
        public const string MISSING_NAME_MESSAGE = "You must specify the name parameter.";


        /// <summary>
        /// Invalid characters in name.
        /// </summary>
        public const string NAME_INVALID_MESSAGE = "Name parameter has invalid format.";

        /// <summary>
        /// No result for name.
        /// </summary>
        public const string NAME_NOT_FOUND_MESSAGE = "Could not find the requested label or identifier.";

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
            Regex NameValidator = new Regex("^[a-zA-Z0-9]+[a-zA-Z0-9\\-_]*$", RegexOptions.IgnoreCase);

            if (String.IsNullOrWhiteSpace(name))
                throw new APIErrorException(400, MISSING_NAME_MESSAGE);
            if(!NameValidator.IsMatch(name))
                throw new APIErrorException(400, NAME_INVALID_MESSAGE);

            TrialTypeInfo result;
            try
            {
                result = await _trialTypeQueryService.Get(name);
            }
            catch (APIInternalException)
            {
                throw new APIErrorException(500, INTERNAL_ERROR_MESSAGE);
            }

            if (result == null)
                throw new APIErrorException(404, NAME_NOT_FOUND_MESSAGE);

            return result;
        }
    }
}
