
namespace NCI.OCPL.Api.Common
{
    /// <summary>
    /// Represents an Exception Raised by the API to be returned to the consumer
    /// </summary>
    public class APIErrorException : System.Exception
    {

        /// <summary>
        /// The HttpStatus to set this response to
        /// </summary>
        /// <returns></returns>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// Creates a new instance of the APIErrorException
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="message"></param>
        public APIErrorException(int httpStatusCode, string message) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}