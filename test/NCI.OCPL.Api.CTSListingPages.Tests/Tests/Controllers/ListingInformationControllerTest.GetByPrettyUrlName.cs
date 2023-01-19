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
        /// Verify correct handling of invalid names.
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

            Assert.Equal(ListingInformationController.MISSING_PRETTY_URL_NAME_MESSAGE, exception.Message);
            Assert.Equal(400, exception.HttpStatusCode);
        }

        /// <summary>
        /// Verify correct handling of pretty URL names containing invalid characters.
        /// </summary>
        [Theory]
        [InlineData(new object[] { "name with spaces" })]
        [InlineData(new object[] { "name-with_underscore" })]
        [InlineData(new object[] { "-name-with-leading-hyphen" })]
        [InlineData(new object[] { "evil-name&quot;<script>alert(\"evil\")</script>" })]
        [InlineData(new object[] { "Robert'); Drop table students;--" })] // Bobby Tables
        public async void GetByPrettyUrlName_InvalidCharacters(string prettyUrlName)
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

            Assert.Equal(ListingInformationController.PRETTY_URL_INVALID_MESSAGE, exception.Message);
            Assert.Equal(400, exception.HttpStatusCode);
        }

        /// <summary>
        /// Verify correct handling of a valid name.
        /// </summary>
        [Theory]
        [InlineData(new object[] { "simplename" })]
        [InlineData(new object[] { "name-with-hyphens" })]
        [InlineData(new object[] { "name-with-numbers-87" })]
        [InlineData(new object[] { "999-435" })]
        public async void GetByPrettyUrlName_ValidName(string theName)
        {
            ListingInfo testRecord = new ListingInfo
            {
                ConceptId = new string[] { "C7884" },
                Name = new NameInfo
                {
                    Label = "Recurrent Adult Brain Tumors",
                    Normalized = "recurrent adult brain tumors"
                },
                PrettyUrlName = "recurrent-adult-brain"
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

            Assert.Equal(ListingInformationController.INTERNAL_ERROR_MESSAGE, exception.Message);
            Assert.Equal(500, exception.HttpStatusCode);
        }

    }
}