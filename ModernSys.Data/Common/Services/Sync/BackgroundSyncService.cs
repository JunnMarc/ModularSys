using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModularSys.Data.Common.Interfaces.Sync;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Services.Sync
{
    /// <summary>
    /// Background service that automatically syncs data at regular intervals
    /// </summary>
    public class BackgroundSyncService : BackgroundService
    {
        private readonly ISyncService _syncService;
        private readonly ILogger<BackgroundSyncService> _logger;
        private readonly TimeSpan _syncInterval;

        public BackgroundSyncService(
            ISyncService syncService,
            ILogger<BackgroundSyncService> logger)
        {
            _syncService = syncService;
            _logger = logger;
            _syncInterval = TimeSpan.FromMinutes(15); // Sync every 15 minutes
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background sync service started. Sync interval: {Interval}", _syncInterval);

            // Wait a bit before first sync to let the app initialize
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Check if online before attempting sync
                    if (await _syncService.IsOnlineAsync())
                    {
                        _logger.LogInformation("Starting automatic background sync");
                        
                        var result = await _syncService.SyncIncrementalAsync(stoppingToken);
                        
                        if (result.Success)
                        {
                            _logger.LogInformation(
                                "Background sync completed successfully. Synced: {Synced}, Failed: {Failed}",
                                result.EntitiesSynced, result.EntitiesFailed);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Background sync completed with errors. Synced: {Synced}, Failed: {Failed}, Error: {Error}",
                                result.EntitiesSynced, result.EntitiesFailed, result.ErrorMessage);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Skipping background sync - offline");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during background sync");
                }

                // Wait for next sync interval
                await Task.Delay(_syncInterval, stoppingToken);
            }

            _logger.LogInformation("Background sync service stopped");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background sync service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
