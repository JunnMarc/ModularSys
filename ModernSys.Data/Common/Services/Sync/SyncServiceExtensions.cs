using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Interfaces.Sync;
using System;

namespace ModularSys.Data.Common.Services.Sync
{
    /// <summary>
    /// Extension methods for registering sync services
    /// </summary>
    public static class SyncServiceExtensions
    {
        /// <summary>
        /// Registers all synchronization services with dependency injection
        /// </summary>
        public static IServiceCollection AddSyncServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register sync-specific DbContext factories
            var localConnectionString = configuration.GetConnectionString("LocalConnection") 
                ?? configuration.GetConnectionString("DefaultConnection");
            var cloudConnectionString = configuration.GetConnectionString("CloudConnection");

            if (!string.IsNullOrEmpty(localConnectionString))
            {
                services.AddDbContextFactory<ModularSysDbContext>(options =>
                    options.UseSqlServer(localConnectionString),
                    ServiceLifetime.Scoped);
            }

            // Register sync services
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddScoped<IChangeTracker, ChangeTracker>();
            services.AddScoped<IConflictResolver, ConflictResolver>();
            services.AddScoped<ISyncService, SyncService>();
            services.AddScoped<RetryService>();

            // Register background sync service if enabled
            var syncEnabled = configuration.GetValue<bool>("SyncSettings:Enabled", false);
            var autoSyncEnabled = configuration.GetValue<bool>("SyncSettings:AutoSyncEnabled", false);
            if (syncEnabled && autoSyncEnabled)
            {
                services.AddHostedService<BackgroundSyncService>();
            }

            return services;
        }

        /// <summary>
        /// Configures sync settings from configuration
        /// </summary>
        public static SyncSettings GetSyncSettings(this IConfiguration configuration)
        {
            var settings = new SyncSettings();
            configuration.GetSection("SyncSettings").Bind(settings);
            return settings;
        }
    }

    /// <summary>
    /// Sync configuration settings
    /// </summary>
    public class SyncSettings
    {
        public bool Enabled { get; set; } = true;
        public string Mode { get; set; } = "Hybrid"; // Local, Cloud, Hybrid
        public bool AutoSyncEnabled { get; set; } = true;
        public int SyncIntervalMinutes { get; set; } = 15;
        public string ConflictResolution { get; set; } = "LastWriteWins";
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public bool UseExponentialBackoff { get; set; } = true;
        public int BatchSize { get; set; } = 100;
        public bool SyncDeletedRecords { get; set; } = true;
        public string[] EnabledEntities { get; set; } = Array.Empty<string>();
    }
}
