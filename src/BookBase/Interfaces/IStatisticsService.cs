using BookBase.Models;

namespace BookBase.Interfaces;

public interface IStatisticsService
{
    Task<DashboardStatistics> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default);
}
