namespace NCI.OCPL.Api.CTSListingPages.Models
{
    /// <summary>
    /// Configuration options for the Drug Dictionary Term API.
    /// </summary>
    public class ListingPageAPIOptions
    {
        /// <summary>
        /// Gets or sets the alias name for the Elasticsearch Collection we will use.
        /// </summary>
        /// <value>The name of the alias.</value>
        public string AliasName { get; set; }
    }
}