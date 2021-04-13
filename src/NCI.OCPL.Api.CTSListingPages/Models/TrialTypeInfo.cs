using Nest;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Specifies a text string to replace with a different text string.
    /// </summary>
    public class TrialTypeInfo
    {
        /// <summary>
        /// Url-friendly version of the text which will be replaced.
        /// </summary>
        [Keyword(Name = "pretty_url_name")]
        public string PrettyUrlName{get;set;}

        /// <summary>
        /// The "identifier" version of the text to be replaced.
        /// </summary>
        [Keyword(Name = "id_string")]
        public string IdString {get;set;}

        /// <summary>
        /// The replacement text.
        /// </summary>
        [Keyword(Name = "label")]
        public string Label { get; set; }
    }
}
