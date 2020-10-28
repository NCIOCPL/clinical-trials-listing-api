namespace NCI.OCPL.Api.CTSListingPages.Models
{
    /// <summary>
    /// Configuration options for the Dynamic Listing Page API.
    /// </summary>
    public class ListingPageAPIOptions
    {
        /// <summary>
        /// Gets or sets the alias name for the TrialTypeInfo Elasticsearch Collection we will use.
        /// </summary>
        /// <value>The name of the TrialTypeInfo alias.</value>
        public string TrialTypeInfoAliasName { get; set; }

        /// <summary>
        /// Gets or sets the alias name for the ListingInfo Elasticsearch Collection we will use.
        /// </summary>
        /// <value>The name of the alias.</value>
        public string ListingInfoAliasName { get; set; }
    }
}