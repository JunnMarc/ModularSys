namespace ModularSys.Core.Models;

public class DashboardStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsersToday { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalRoles { get; set; }
    public decimal UserGrowthPercentage { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class UserGrowthData
{
    public string Month { get; set; } = string.Empty;
    public int NewUsers { get; set; }
    public int TotalUsers { get; set; }
    public DateTime Date { get; set; }
}

public class DepartmentStats
{
    public string DepartmentName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int ActiveUsers { get; set; }
    public decimal Percentage { get; set; }
}

public class RecentActivity
{
    public string Activity { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = "Default";
}

public class SystemHealthStats
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public long DatabaseSize { get; set; }
    public TimeSpan Uptime { get; set; }
    public int ActiveConnections { get; set; }
    public DateTime LastBackup { get; set; }
    public bool IsDatabaseOnline { get; set; }
    public string DatabaseConnectionType { get; set; } = "Unknown";
    public string? DatabaseErrorMessage { get; set; }
    public bool IsHealthy => CpuUsage < 80 && MemoryUsage < 85 && IsDatabaseOnline;
}

public class RoleDistribution
{
    public string RoleName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public decimal Percentage { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class SecurityMetrics
{
    public int TotalLoginAttempts { get; set; }
    public int FailedLoginAttempts { get; set; }
    public int SuccessfulLogins { get; set; }
    public int ActiveSessions { get; set; }
    public int SuspiciousActivities { get; set; }
    public DateTime LastSecurityScan { get; set; }
    public decimal SecurityScore => TotalLoginAttempts > 0 
        ? (decimal)SuccessfulLogins / TotalLoginAttempts * 100 
        : 100;
}

public class PerformanceMetrics
{
    public double AverageResponseTime { get; set; }
    public int TotalRequests { get; set; }
    public int FailedRequests { get; set; }
    public double RequestsPerSecond { get; set; }
    public long TotalDataTransferred { get; set; }
    public DateTime MeasurementStart { get; set; }
    public double SuccessRate => TotalRequests > 0 
        ? (double)(TotalRequests - FailedRequests) / TotalRequests * 100 
        : 100;
}
