namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Contains the return data for retrieving a collection of EvsRecord objects.
    /// </summary>
    public class OverrideResults
    {
        /// <summary>
        /// Contains metadata about the collection.
        /// </summary>
        public ResultsMetadata Meta;

        /// <summary>
        /// An array of zero or more OverrideRecord objects.
        /// </summary>
        public OverrideRecord[] Results;
    }
}
