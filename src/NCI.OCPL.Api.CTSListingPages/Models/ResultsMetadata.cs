namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Metadata about a DrugResults object.
    /// </summary>
    public class ResultsMetadata
    {
        /// <summary>
        /// The total number of records available matching the retrieval operation.
        /// </summary>
        public int TotalResults;

        /// <summary>
        /// Zero-based offset into the total collection of matching records for the parent object's Results collection.
        /// </summary>
        public int From;
    }
}
