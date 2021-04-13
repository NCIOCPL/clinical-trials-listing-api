namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Contains the return data for retrieving a collection of
    /// listing information objects.
    /// </summary>
    public class ListingResults
    {
        /// <summary>
        /// Contains metadata about the collection.
        /// </summary>
        public ResultsMetadata Meta;

        /// <summary>
        /// An array of zero or more ListingInfo objects.
        /// </summary>
        public ListingInfo[] Results;
    }
}
