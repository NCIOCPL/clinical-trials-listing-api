using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Controllers;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public partial class OverrideControllerTest
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
            Mock<IOverridesQueryService> querySvc = new Mock<IOverridesQueryService>();
            querySvc.Setup(
                svc => svc.GetByPrettyUrlName(
                    It.IsAny<string>()
                )
            )
            .Returns(Task.FromResult(new OverrideRecord()));

            OverrideController controller = new OverrideController(NullLogger<OverrideController>.Instance, querySvc.Object);

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

            OverrideRecord testRecord = new OverrideRecord
            {
                UniqueId = "C7884",
                ConceptId = new string[] { "C7884" },
                Name = new NameInfo
                {
                    Label = "Recurrent Adult Brain Tumors",
                    Normalized = "recurrent adult brain tumors"
                },
                PrettyUrlName = "recurrent-adult-brain"
            };

            Mock<IOverridesQueryService> querySvc = new Mock<IOverridesQueryService>();
            querySvc.Setup(
                svc => svc.GetByPrettyUrlName(
                    It.IsAny<string>()
                )
            )
            .Returns(Task.FromResult(testRecord));

            // Call the controller.
            OverrideController controller = new OverrideController(NullLogger<OverrideController>.Instance, querySvc.Object);
            OverrideRecord actual = await controller.GetByPrettyUrlName(theName);

            Assert.Equal(testRecord, actual);

            // Verify the query layer is called:
            //  a) with the passed value.
            //  b) exactly once.
            querySvc.Verify(
                svc => svc.GetByPrettyUrlName(theName),
                Times.Once,
                $"IOverridesQueryService:GetByPrettyUrlName() should be called once, with prettyUrlName = '{theName}"
            );

        }

        /// <summary>
        /// Verify correct handling of an internal error in the service layer.
        /// </summary>
        [Fact]
        public async void GetByPrettyUrlName_ServiceFailure()
        {
            const string theName = "recurrent-adult-brain";

            Mock<IOverridesQueryService> querySvc = new Mock<IOverridesQueryService>();
            querySvc.Setup(
                svc => svc.GetByPrettyUrlName(
                    It.IsAny<string>()
                )
            )
            .Throws(new APIInternalException("Kaboom!"));

            // Call the controller, we're not expecting a result, so don't save it.
            OverrideController controller = new OverrideController(NullLogger<OverrideController>.Instance, querySvc.Object);

            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.GetByPrettyUrlName(theName)
            );

            Assert.Equal("Errors occured.", exception.Message);
            Assert.Equal(500, exception.HttpStatusCode);
        }

    }
}