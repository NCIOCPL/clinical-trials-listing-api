using Newtonsoft.Json;

namespace NCI.OCPL.Api.Common
{
    /// <summary>
    /// Represents a Error Message to be returned to the client
    /// </summary>
    public class ErrorMessage
    {
        /// <summary>
        /// The message to display
        /// </summary>
        /// <returns></returns>
        public string Message { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Drug-Dictionary-API.Models.ErrorMessage"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Drug-Dictionary-API.Models.ErrorMessage"/>.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}