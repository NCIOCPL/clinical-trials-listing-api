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
        public string PrettyUrlName{get;set;}

        /// <summary>
        /// The "identifier" version of the text to be replaced.
        /// </summary>
        public string IdString {get;set;}

        /// <summary>
        /// The replacement text.
        /// </summary>
        public string Label { get; set; }
    }
}
