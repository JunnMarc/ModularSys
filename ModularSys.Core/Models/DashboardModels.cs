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
    public bool IsHealthy => CpuUsage < 80 && MemoryUsage < 85;
}
