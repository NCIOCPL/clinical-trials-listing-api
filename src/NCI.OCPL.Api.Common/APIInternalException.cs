namespace NCI.OCPL.Api.Common
{
    /// <summary>
    /// Represents an internal error encountered by the API.
    /// This exception is intended to be thrown by API services and caught by controllers.
    /// It is **NOT** intended to be returned to the API's consumer.
    /// </summary>
    public class APIInternalException : System.Exception
    {
        /// <summary>
        /// Creates a new instance of the APIErrorException
        /// </summary>
        /// <param name="message"></param>
        public APIInternalException(string message) :
        base(message) {}
    }
}