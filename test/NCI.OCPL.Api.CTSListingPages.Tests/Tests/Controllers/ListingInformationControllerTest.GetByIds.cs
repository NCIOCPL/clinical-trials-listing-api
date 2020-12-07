using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Controllers;
using System;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public partial class ListingInformationControllerTest
    {
        /// <summary>
        /// Verify correcting handling of invalid c-codes.
        /// </summary>
        [Theory]
        [InlineData(new object[] { null })] // Null...
        [InlineData(new object[] { new string[] { } })] // Empty array
        [InlineData(new object[] { new string[] { "" } })] // Single empty string in array
        [InlineData(new object[] { new string[] { null } })] // Single null in array
        [InlineData(new object[] { new string[] { "C1234", "", "C5678" } })] // Array with an empty string among other valud strings
        [InlineData(new object[] { new string[] { "C1234", null, "C5678" } })] // Array with a null value among other valid strings
        [InlineData(new object[] { new string[] { "C1234", null, "" } })] // Array with empty string and null value among other valid strings

        public async void GetByIds_InvalidCCodes(string[] ccode)
        {
            Mock<IListingInfoQueryService> querySvc = new Mock<IListingInfoQueryService>();
            querySvc.Setup(
                svc => svc.GetByIds(
                    It.IsAny<string[]>()
                )
            )
            .Returns(Task.FromResult(new ListingInfo[] { }));

            ListingInformationController controller = new ListingInformationController(NullLogger<ListingInformationController>.Instance, querySvc.Object);

            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.GetByIds(ccode)
            );

            querySvc.Verify(
                svc => svc.GetByIds(It.IsAny<string[]>()),
                Times.Never
            );

            Assert.Equal("You must specify at least one ccode parameter.", exception.Message);
            Assert.Equal(400, exception.HttpStatusCode);
        }

        /// <summary>
        /// Verify correct handling of found ListingInfo array.
        /// </summary>
        [Fact]
        public async void GetByIds_Found()
        {
            string[] ccodes = new string[] { "C3016", "C8578", "C9092", "C115270" };

            ListingInfo[] testResults = new ListingInfo[] {
                new ListingInfo
                {
                    ConceptId = new string[] { "C3016", "C8578", "C9092", "C115270" },
                    Name = new NameInfo
                    {
                        Label = "Ependymoma",
                        Normalized = "ependymoma"
                    },
                    PrettyUrlName = "ependymoma"
                }
            };

            Mock<IListingInfoQueryService> querySvc = new Mock<IListingInfoQueryService>();
            querySvc.Setup(
                svc => svc.GetByIds(
                    It.IsAny<string[]>()
                )
            )
            .Returns(Task.FromResult(testResults));

            // Call the controller.
            ListingInformationController controller = new ListingInformationController(NullLogger<ListingInformationController>.Instance, querySvc.Object);
            ListingInfo actual = await controller.GetByIds(ccodes);

            // Verify the object returned from the service layer isn't modified.
            Assert.Equal(testResults.First(), actual);

            // Verify the query layer is called:
            //  a) with the passed value.
            //  b) exactly once.
            querySvc.Verify(
                svc => svc.GetByIds(ccodes),
                Times.Once,
                $"IListingInfoQueryService:GetByIds() should be called once, with ccode = '{string.Join(", ", ccodes)}'"
            );

        }

        /// <summary>
        /// Verify correct handling of not found ListingResults.
        /// </summary>
        [Fact]
        public async void GetByIds_NotFound()
        {
            string[] ccodes = new string[] { "C0000" };

            ListingInfo[] testResults = null;

            Mock<IListingInfoQueryService> querySvc = new Mock<IListingInfoQueryService>();
            querySvc.Setup(
                svc => svc.GetByIds(
                    It.IsAny<string[]>()
                )
            )
            .Returns(Task.FromResult(testResults));

            // Call the controller.
            ListingInformationController controller = new ListingInformationController(NullLogger<ListingInformationController>.Instance, querySvc.Object);
            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.GetByIds(ccodes)
            );

            Assert.Equal("Could not find codes 'C0000'.", exception.Message);
            Assert.Equal(404, exception.HttpStatusCode);
        }

        /// <summary>
        /// Verify correct handling of multiple ListingInfos found.
        /// </summary>
        [Fact]
        public async void GetByIds_Multiple()
        {
            string[] ccodes = new string[] { "C4872" };

            ListingInfo[] testResults = new ListingInfo[]
            {
                new ListingInfo {
                    ConceptId = new string[] { "C4872" },
                    Name = new NameInfo
                    {
                        Label = "Breast Cancer",
                        Normalized = "breast cancer"
                    },
                    PrettyUrlName = "breast-cancer"
                },
                new ListingInfo {
                    ConceptId = new string[] { "C4872" },
                    Name = new NameInfo
                    {
                        Label = "Breast Cancer",
                        Normalized = "breast cancer"
                    },
                    PrettyUrlName = "breast-cancer"
                }
            };

        Mock<IListingInfoQueryService> querySvc = new Mock<IListingInfoQueryService>();
            querySvc.Setup(
                svc => svc.GetByIds(
                    It.IsAny<string[]>()
                )
            )
            .Returns(Task.FromResult(testResults));

            // Call the controller.
            ListingInformationController controller = new ListingInformationController(NullLogger<ListingInformationController>.Instance, querySvc.Object);
            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.GetByIds(ccodes)
            );

            Assert.Equal("Multiple records found for codes 'C4872'.", exception.Message);
            Assert.Equal(409, exception.HttpStatusCode);
        }

        /// <summary>
        /// Verify correct handling of an internal error in the service layer.
        /// </summary>
        [Fact]
        public async void GetByIds_ServiceFailure()
        {
            string[] ccodes = new string[] { "C1234", "C5678" };

            Mock<IListingInfoQueryService> querySvc = new Mock<IListingInfoQueryService>();
            querySvc.Setup(
                svc => svc.GetByIds(
                    It.IsAny<string[]>()
                )
            )
            .Throws(new APIInternalException("Kaboom!"));

            // Call the controller, we're not expecting a result, so don't save it.
            ListingInformationController controller = new ListingInformationController(NullLogger<ListingInformationController>.Instance, querySvc.Object);

            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.GetByIds(ccodes)
            );

            Assert.Equal("Errors occured.", exception.Message);
            Assert.Equal(500, exception.HttpStatusCode);
        }

    }
}