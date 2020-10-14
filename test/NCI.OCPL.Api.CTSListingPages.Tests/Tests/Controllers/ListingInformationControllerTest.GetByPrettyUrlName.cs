using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Controllers;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public partial class ListingInformationControllerTest
    {
        /// <summary>
        /// Verify correcting handling of invalid names.
        /// </summary>
        [Theory]
        [InlineData(new object[] { null })]
        [InlineData(new object[] { "" })]       // Can't use String.Empty because it's a property instead of a constant.
        [InlineData(new object[] { "    " })]
        public async void GetByPrettyUrlName_InvalidName(string prettyUrlName)
        {
            Mock<IListingInfoQueryService> querySvc = new Mock<IListingInfoQueryService>();
            querySvc.Setup(
                svc => svc.GetByPrettyUrlName(
                    It.IsAny<string>()
                )
            )
            .Returns(Task.FromResult(new ListingInfo()));

            ListingInformationController controller = new ListingInformationController(NullLogger<ListingInformationController>.Instance, querySvc.Object);

            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.GetByPrettyUrlName(prettyUrlName)
            );

            querySvc.Verify(
                svc => svc.GetByPrettyUrlName(It.IsAny<string>()),
                Times.Never
            );

            Assert.Equal("You must specify the prettyUrlName parameter.", exception.Message);
            Assert.Equal(400, exception.HttpStatusCode);
        }

        /// <summary>
        /// Verify correct handling of a valid name.
        /// </summary>
        [Fact]
        public async void GetByPrettyUrlName_ValidName()
        {
            const string theName = "recurrent-adult-brain";

            ListingInfo testRecord = new ListingInfo
            {
                UniqueId = "C7884",
                ConceptId = new string[] { "C7884" },
                Name = new NameInfo
                {
                    Label = "Recurrent Adult Brain Tumors",
                    Normalized = "recurrent adult brain tumors"
                },
                PrettyUrlName = "recurrent-adult-brain",
                ExactMatch = true
            };

            Mock<IListingInfoQueryService> querySvc = new Mock<IListingInfoQueryService>();
            querySvc.Setup(
                svc => svc.GetByPrettyUrlName(
                    It.IsAny<string>()
                )
            )
            .Returns(Task.FromResult(testRecord));

            // Call the controller.
            ListingInformationController controller = new ListingInformationController(NullLogger<ListingInformationController>.Instance, querySvc.Object);
            ListingInfo actual = await controller.GetByPrettyUrlName(theName);

            // Verify the object returned from the service layer isn't modified.
            Assert.Equal(testRecord, actual);

            // Verify the query layer is called:
            //  a) with the passed value.
            //  b) exactly once.
            querySvc.Verify(
                svc => svc.GetByPrettyUrlName(theName),
                Times.Once,
                $"IListingInfoQueryService:GetByPrettyUrlName() should be called once, with prettyUrlName = '{theName}"
            );

        }

        /// <summary>
        /// Verify correct handling of an internal error in the service layer.
        /// </summary>
        [Fact]
        public async void GetByPrettyUrlName_ServiceFailure()
        {
            const string theName = "recurrent-adult-brain";

            Mock<IListingInfoQueryService> querySvc = new Mock<IListingInfoQueryService>();
            querySvc.Setup(
                svc => svc.GetByPrettyUrlName(
                    It.IsAny<string>()
                )
            )
            .Throws(new APIInternalException("Kaboom!"));

            // Call the controller, we're not expecting a result, so don't save it.
            ListingInformationController controller = new ListingInformationController(NullLogger<ListingInformationController>.Instance, querySvc.Object);

            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.GetByPrettyUrlName(theName)
            );

            Assert.Equal("Errors occured.", exception.Message);
            Assert.Equal(500, exception.HttpStatusCode);
        }

    }
}