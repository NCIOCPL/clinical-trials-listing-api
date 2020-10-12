namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Contains the return data for retrieving a collection of EvsRecord objects.
    /// </summary>
    public class EvsResults
    {
        /// <summary>
        /// Contains metadata about the collection.
        /// </summary>
        public ResultsMetadata Meta;

        /// <summary>
        /// An array of zero or more EvsRecord objects.
        /// </summary>
        public EvsRecord[] Results;
    }
}
