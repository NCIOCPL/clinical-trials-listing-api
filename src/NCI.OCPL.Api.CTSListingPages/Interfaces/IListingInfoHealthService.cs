using NCI.OCPL.Api.Common;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Alias for IHealthCheckService, used to distinguish an instance for checking
    /// the trial type index's health.
    /// </summary>
    public interface IListingInfoHealthService : IHealthCheckService
    {
    }
}