using Xunit;

using NCI.OCPL.Api.CTSListingPages.Services;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public partial class ESListingInfoQueryServiceTest
    {
        /// <summary>
        /// Verify the AllRequestedCodesPresent helper method returns correctly when all requested codes
        /// are present.
        /// </summary>
        [Theory]
        [InlineData(new string[] { "C123" }, new string[] { "C123" })] // One code
        [InlineData(new string[] { "C123", "C456", "C789" }, new string[] { "C123", "C456", "C789" })] // Multiple codes
        [InlineData(new string[] { "C123" }, new string[] { "C123", "C456", "C789" })] // Subset of codes
        [InlineData(new string[] { "C456", "C789" }, new string[] { "C123", "C456", "C789" })] // Subset of codes
        public void AllRequestedCodesPresent_AllValid(string[] requested, string[] actual)
        {
            bool result = ESListingInfoQueryService.AllRequestedCodesPresent(requested, actual);
            Assert.True(result);
        }

        /// <summary>
        /// Verify the AllRequestedCodesPresent helper method returns correctly when all requested codes
        /// are missing from the actual.
        /// </summary>
        [Theory]
        [InlineData(new string[] { "C123", "C456", "C789" }, new string[] { "C123" })] // More codes than data
        [InlineData(new string[] { "C123", "C456", "C789" }, new string[] { "C123", "C789" })] // More codes than data
        [InlineData(new string[] { "C123" }, new string[] { "C456" })] // Code not present
        [InlineData(new string[] { "C456", "C789" }, new string[] { "C123", "C456" })] // Multiple codes
        [InlineData(new string[] { "C123", "C456", "C789" }, new string[] { "C123", "C456" })] // All but one present
        public void AllRequestedCodesPresent_MissingCodes(string[] requested, string[] actual)
        {
            bool result = ESListingInfoQueryService.AllRequestedCodesPresent(requested, actual);
            Assert.False(result);
        }
    }
}
