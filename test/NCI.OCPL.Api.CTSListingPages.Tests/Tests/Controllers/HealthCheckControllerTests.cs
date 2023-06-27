using System;

using Microsoft.Extensions.Logging.Testing;

using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.CTSListingPages.Controllers;


namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public class HealthCheckControllerTests
    {
        /// <summary>
        /// Handle the listing info healthcheck reporting an unhealthy status.
        /// </summary>
        [Fact]
        public async void ListingInfoIsUnhealthy()
        {
            var logger = NullLogger<HealthCheckController>.Instance;

            // Listing info unhealthy
            var listingCheckSvc = new Mock<IListingInfoHealthService>();
            listingCheckSvc.Setup(
                svc => svc.IndexIsHealthy()
            )
            .ReturnsAsync(false);

            // Trial type is healthy
            var trialTypeCheckSvc = new Mock<ITrialTypeHealthService>();
            trialTypeCheckSvc.Setup(
                svc => svc.IndexIsHealthy()
            )
            .ReturnsAsync(true);

            var controller = new HealthCheckController(
                logger,
                listingCheckSvc.Object,
                trialTypeCheckSvc.Object
            );

            var exception = await Assert.ThrowsAsync<APIErrorException>(() => controller.IsHealthy());

            Assert.Equal(500, exception.HttpStatusCode);
            Assert.Equal(HealthCheckController.UNHEALTHY_STATUS, exception.Message);
        }


        /// <summary>
        /// Handle the listing info healthcheck reporting an unhealthy status.
        /// </summary>
        [Fact]
        public async void TrialTypeIsUnhealthy()
        {
            var logger = NullLogger<HealthCheckController>.Instance;

            // Listing info healthy
            var listingCheckSvc = new Mock<IListingInfoHealthService>();
            listingCheckSvc.Setup(
                svc => svc.IndexIsHealthy()
            )
            .ReturnsAsync(true);

            // Trial type is unhealthy
            var trialTypeCheckSvc = new Mock<ITrialTypeHealthService>();
            trialTypeCheckSvc.Setup(
                svc => svc.IndexIsHealthy()
            )
            .ReturnsAsync(false);

            var controller = new HealthCheckController(
                logger,
                listingCheckSvc.Object,
                trialTypeCheckSvc.Object
            );

            var exception = await Assert.ThrowsAsync<APIErrorException>(() => controller.IsHealthy());

            Assert.Equal(500, exception.HttpStatusCode);
            Assert.Equal(HealthCheckController.UNHEALTHY_STATUS, exception.Message);
        }

        /// <summary>
        /// Handle the health service failing.
        /// </summary>
        [Fact]
        public async void IsVeryUnhealthy()
        {
            var logger = NullLogger<HealthCheckController>.Instance;

            var listingCheckSvc = new Mock<IListingInfoHealthService>();
            listingCheckSvc.Setup(
                svc => svc.IndexIsHealthy()
            )
            .ThrowsAsync(new Exception("kaboom!"));

            var trialTypeCheckSvc = new Mock<ITrialTypeHealthService>();
            trialTypeCheckSvc.Setup(
                svc => svc.IndexIsHealthy()
            )
            .ThrowsAsync(new Exception("kapow!"));


            var controller = new HealthCheckController(
                logger,
                listingCheckSvc.Object,
                trialTypeCheckSvc.Object
            );

            var exception = await Assert.ThrowsAsync<APIErrorException>(
                () => controller.IsHealthy()
            );

            Assert.Equal(500, exception.HttpStatusCode);
            Assert.Equal(HealthCheckController.UNHEALTHY_STATUS, exception.Message);
        }


        /// <summary>
        /// Handle the healthcheck reporting a healthy status.
        /// </summary>
        [Fact]
        public async void IsHealthy()
        {
            var logger = NullLogger<HealthCheckController>.Instance;

            // Listing info healthy
            var listingCheckSvc = new Mock<IListingInfoHealthService>();
            listingCheckSvc.Setup(
                svc => svc.IndexIsHealthy()
            )
            .ReturnsAsync(true);

            // Trial type is healthy
            var trialTypeCheckSvc = new Mock<ITrialTypeHealthService>();
            trialTypeCheckSvc.Setup(
                svc => svc.IndexIsHealthy()
            )
            .ReturnsAsync(true);

            var controller = new HealthCheckController(
                logger,
                listingCheckSvc.Object,
                trialTypeCheckSvc.Object
            );

            string actual = await controller.IsHealthy();

            listingCheckSvc.Verify(
                svc => svc.IndexIsHealthy(),
                Times.Once
            );

            trialTypeCheckSvc.Verify(
                svc => svc.IndexIsHealthy(),
                Times.Once
            );

            Assert.Equal(HealthCheckController.HEALTHY_STATUS, actual);
        }

    }
}