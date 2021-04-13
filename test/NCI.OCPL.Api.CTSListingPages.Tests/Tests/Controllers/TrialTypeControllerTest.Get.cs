using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Controllers;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    /// <summary>
    /// Test the functionality of the Get endpoint of the TrialTypeController.
    /// </summary>
    public partial class TrialTypeControllerTest
    {
        /// <summary>
        /// Verify correcting handling of invalid names.
        /// </summary>
        [Theory]
        [InlineData(new object[] { null })]
        [InlineData(new object[] { "" })]       // Can't use String.Empty because it's a property instead of a constant.
        [InlineData(new object[] { "    " })]
        public async void Get_InvalidName(string name)
        {
            Mock<ITrialTypeQueryService> querySvc = new Mock<ITrialTypeQueryService>();
            querySvc.Setup(
                svc => svc.Get(
                    It.IsAny<string>()
                )
            )
            .Returns(Task.FromResult(new TrialTypeInfo()));

            TrialTypeController controller = new TrialTypeController(NullLogger<TrialTypeController>.Instance, querySvc.Object);

            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.Get(name)
            );

            querySvc.Verify(
                svc => svc.Get(It.IsAny<string>()),
                Times.Never
            );

            Assert.Equal("You must specify the name parameter.", exception.Message);
            Assert.Equal(400, exception.HttpStatusCode);
        }

        /// <summary>
        /// Verify correct handling of a valid name.
        /// </summary>
        [Fact]
        public async void Get_ValidName()
        {
            const string theName = "basic-science";

            TrialTypeInfo testTrialTypeInfo = new TrialTypeInfo
            {
                PrettyUrlName = "basic-science",
                IdString = "basic_science",
                Label = "Basic Science"
            };

            Mock<ITrialTypeQueryService> querySvc = new Mock<ITrialTypeQueryService>();
            querySvc.Setup(
                svc => svc.Get(
                    It.IsAny<string>()
                )
            )
            .Returns(Task.FromResult(testTrialTypeInfo));

            // Call the controller.
            TrialTypeController controller = new TrialTypeController(NullLogger<TrialTypeController>.Instance, querySvc.Object);
            TrialTypeInfo actual = await controller.Get(theName);

            Assert.Equal(testTrialTypeInfo, actual);

            // Verify the query layer is called:
            //  a) with the passed value.
            //  b) exactly once.
            querySvc.Verify(
                svc => svc.Get(theName),
                Times.Once,
                $"ITrialTypeQueryService:Get() should be called once, with name = '{theName}"
            );

        }

        /// <summary>
        /// Verify correct handling of an internal error in the service layer.
        /// </summary>
        [Fact]
        public async void Get_ServiceFailure()
        {
            const string theName = "health-services-research";

            Mock<ITrialTypeQueryService> querySvc = new Mock<ITrialTypeQueryService>();
            querySvc.Setup(
                svc => svc.Get(
                    It.IsAny<string>()
                )
            )
            .Throws(new APIInternalException("Kaboom!"));

            // Call the controller, we're not expecting a result, so don't save it.
            TrialTypeController controller = new TrialTypeController(NullLogger<TrialTypeController>.Instance, querySvc.Object);

            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.Get(theName)
            );

            Assert.Equal("Errors occured.", exception.Message);
            Assert.Equal(500, exception.HttpStatusCode);
        }
    }
}