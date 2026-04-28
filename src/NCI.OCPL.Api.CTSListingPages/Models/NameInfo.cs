namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Child class of ListingInfo containing both the original and normalized versions of
    /// the item's label..
    /// </summary>
    public class NameInfo
    {
        /// <summary>
        /// The name for the term.
        /// </summary>
        public string Label {get;set;}

        /// <summary>
        /// A normalized form of the Label value.
        /// </summary>
        public string Normalized { get; set; }
    }
}
