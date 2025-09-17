using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Interfaces;
using ModularSys.Core.Models;
using ModularSys.Data.Common.Db;
using System.Diagnostics;

namespace ModularSys.Core.Services;

public class DashboardService : IDashboardService
{
    private readonly ModularSysDbContext _db;
    private static readonly DateTime _appStartTime = DateTime.UtcNow;

    public DashboardService(ModularSysDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync();
        var totalDepartments = await _db.Departments.CountAsync();
        var totalRoles = await _db.Roles.CountAsync();

        // Calculate active users (users who are not deleted)
        var activeUsers = await _db.Users.IgnoreQueryFilters().CountAsync(u => !u.IsDeleted);

        // Simple growth calculation based on total users vs departments ratio
        var avgUsersPerDept = totalDepartments > 0 ? (decimal)totalUsers / totalDepartments : 0;
        var growthPercentage = Math.Round(avgUsersPerDept * 10, 1); // Simple growth indicator

        return new DashboardStats
        {
            TotalUsers = totalUsers,
            ActiveUsersToday = activeUsers,
            NewUsersThisMonth = totalUsers, // Show total users instead
            TotalDepartments = totalDepartments,
            TotalRoles = totalRoles,
            UserGrowthPercentage = growthPercentage
        };
    }

    public async Task<List<UserGrowthData>> GetUserGrowthDataAsync(int months = 12)
    {
        var result = new List<UserGrowthData>();
        var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync();
        
        // Create simple mock data for now to avoid complex date queries
        var now = DateTime.UtcNow;
        for (int i = months - 1; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            
            result.Add(new UserGrowthData
            {
                Month = monthStart.ToString("MMM yyyy"),
                NewUsers = i == 0 ? totalUsers : Math.Max(0, totalUsers - i), // Simple mock data
                TotalUsers = totalUsers,
                Date = monthStart
            });
        }

        return result;
    }

    public async Task<List<DepartmentStats>> GetDepartmentStatsAsync()
    {
        var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync();
        
        var departments = await _db.Departments.ToListAsync();
        var departmentStats = new List<DepartmentStats>();
        
        foreach (var dept in departments)
        {
            var userCount = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.DepartmentId == dept.DepartmentId);
            
            departmentStats.Add(new DepartmentStats
            {
                DepartmentName = dept.DepartmentName,
                UserCount = userCount,
                ActiveUsers = userCount,
                Percentage = totalUsers > 0 ? (decimal)userCount / totalUsers * 100 : 0
            });
        }

        return departmentStats.OrderByDescending(d => d.UserCount).ToList();
    }

    public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 10)
    {
        var activities = new List<RecentActivity>();

        // Get recent user registrations (simplified)
        var recentUsers = await _db.Users.IgnoreQueryFilters()
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        foreach (var user in recentUsers)
        {
            activities.Add(new RecentActivity
            {
                Activity = "User Registration",
                UserName = $"{user.FirstName} {user.LastName}",
                Description = $"User {user.Username} joined the system",
                Timestamp = user.CreatedAt,
                Icon = "PersonAdd",
                Color = "Success"
            });
        }

        // Add system activity
        activities.Add(new RecentActivity
        {
            Activity = "System Startup",
            UserName = "System",
            Description = "ModularSys application started",
            Timestamp = _appStartTime,
            Icon = "PlayArrow",
            Color = "Primary"
        });

        return activities.OrderByDescending(a => a.Timestamp).Take(count).ToList();
    }

    public async Task<SystemHealthStats> GetSystemHealthAsync()
    {
        // Get basic system info
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - _appStartTime;

        // Simulate some metrics (in a real app, you'd get these from system monitoring)
        var random = new Random();
        
        return new SystemHealthStats
        {
            CpuUsage = random.Next(10, 30), // Simulate low CPU usage
            MemoryUsage = (process.WorkingSet64 / 1024.0 / 1024.0) / 1024.0 * 100, // Convert to percentage
            DatabaseSize = await GetDatabaseSizeAsync(),
            Uptime = uptime,
            ActiveConnections = random.Next(5, 15),
            LastBackup = DateTime.UtcNow.AddHours(-random.Next(1, 24))
        };
    }

    private async Task<long> GetDatabaseSizeAsync()
    {
        try
        {
            // Get approximate database size by counting records
            var userCount = await _db.Users.CountAsync();
            var roleCount = await _db.Roles.CountAsync();
            var deptCount = await _db.Departments.CountAsync();
            
            // Simulate size calculation (in a real app, you'd query actual DB size)
            return (userCount + roleCount + deptCount) * 1024; // Approximate size in bytes
        }
        catch
        {
            return 0;
        }
    }
}
