using ModularSys.Core.Models;

namespace ModularSys.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<List<UserGrowthData>> GetUserGrowthDataAsync(int months = 12);
    Task<List<DepartmentStats>> GetDepartmentStatsAsync();
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 10);
    Task<SystemHealthStats> GetSystemHealthAsync();
    Task<List<RoleDistribution>> GetRoleDistributionAsync();
    Task<SecurityMetrics> GetSecurityMetricsAsync();
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();
}
