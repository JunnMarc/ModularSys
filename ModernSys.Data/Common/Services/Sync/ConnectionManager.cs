using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Interfaces.Sync;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Services.Sync
{
    /// <summary>
    /// Manages connections between local and cloud SQL Server databases
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConnectionManager> _logger;
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;
        private ConnectionMode _currentMode = ConnectionMode.Hybrid;
        private bool _cloudAvailable = true;
        private DateTime _lastCloudCheck = DateTime.MinValue;
        private readonly TimeSpan _cloudCheckInterval = TimeSpan.FromMinutes(2);
        
        private string LocalConnectionString => _configuration.GetConnectionString("LocalConnection") 
            ?? _configuration.GetConnectionString("DefaultConnection")!;
        
        private string CloudConnectionString => _configuration.GetConnectionString("CloudConnection")!;

        // Event to notify when connection status changes
        public event Action<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public ConnectionManager(
            IConfiguration configuration,
            ILogger<ConnectionManager> logger,
            IDbContextFactory<ModularSysDbContext> contextFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _contextFactory = contextFactory;
            
            // Start background monitoring
            _ = MonitorConnectionStatusAsync();
        }

        public ModularSysDbContext GetLocalContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ModularSysDbContext>();
            optionsBuilder.UseSqlServer(LocalConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
            return new ModularSysDbContext(optionsBuilder.Options);
        }

        public ModularSysDbContext GetCloudContext()
        {
            if (string.IsNullOrEmpty(CloudConnectionString))
            {
                throw new InvalidOperationException("Cloud connection string is not configured");
            }
            
            var optionsBuilder = new DbContextOptionsBuilder<ModularSysDbContext>();
            optionsBuilder.UseSqlServer(CloudConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
            return new ModularSysDbContext(optionsBuilder.Options);
        }

        public async Task<bool> TestLocalConnectionAsync()
        {
            try
            {
                using var context = GetLocalContext();
                return await context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to local database");
                return false;
            }
        }

        public async Task<bool> TestCloudConnectionAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CloudConnectionString))
                {
                    _logger.LogWarning("Cloud connection string is not configured");
                    return false;
                }
                
                using var context = GetCloudContext();
                return await context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to cloud database");
                return false;
            }
        }

        public ConnectionMode GetConnectionMode()
        {
            return _currentMode;
        }

        public void SetConnectionMode(ConnectionMode mode)
        {
            _logger.LogInformation("Connection mode changed from {OldMode} to {NewMode}", _currentMode, mode);
            _currentMode = mode;
        }

        public async Task<bool> IsCloudReachableAsync()
        {
            try
            {
                // First check internet connectivity
                if (!await IsInternetAvailableAsync())
                {
                    return false;
                }
                
                // Then test cloud database connection
                return await TestCloudConnectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cloud reachability");
                return false;
            }
        }

        private async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                // Try to ping a reliable host
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", 3000); // Google DNS
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                // If ping fails, try alternative check
                try
                {
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync("1.1.1.1", 3000); // Cloudflare DNS
                    return reply.Status == IPStatus.Success;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Monitors cloud connection status and automatically switches to local-only mode if cloud is unavailable
        /// </summary>
        private async Task MonitorConnectionStatusAsync()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(_cloudCheckInterval);
                    
                    // Check if enough time has passed since last check
                    if (DateTime.UtcNow - _lastCloudCheck < _cloudCheckInterval)
                        continue;
                    
                    _lastCloudCheck = DateTime.UtcNow;
                    
                    // Test cloud connectivity
                    var wasAvailable = _cloudAvailable;
                    _cloudAvailable = await IsCloudReachableAsync();
                    
                    // If status changed, notify and adjust mode
                    if (wasAvailable != _cloudAvailable)
                    {
                        if (_cloudAvailable)
                        {
                            _logger.LogInformation("Cloud connection restored. Switching to Hybrid mode.");
                            SetConnectionMode(ConnectionMode.Hybrid);
                            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
                            {
                                IsCloudAvailable = true,
                                ConnectionMode = ConnectionMode.Hybrid,
                                Message = "Cloud connection restored. Sync enabled.",
                                Timestamp = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            _logger.LogWarning("Cloud connection lost. Switching to Local-only mode.");
                            SetConnectionMode(ConnectionMode.Local);
                            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
                            {
                                IsCloudAvailable = false,
                                ConnectionMode = ConnectionMode.Local,
                                Message = "Cloud unavailable. Operating in offline mode.",
                                Timestamp = DateTime.UtcNow
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in connection monitoring");
                }
            }
        }

        /// <summary>
        /// Gets the current cloud availability status
        /// </summary>
        public bool IsCloudAvailable() => _cloudAvailable;

        /// <summary>
        /// Forces an immediate cloud connectivity check
        /// </summary>
        public async Task<bool> CheckCloudStatusNowAsync()
        {
            _cloudAvailable = await IsCloudReachableAsync();
            _lastCloudCheck = DateTime.UtcNow;
            return _cloudAvailable;
        }

        protected virtual void OnConnectionStatusChanged(ConnectionStatusChangedEventArgs e)
        {
            ConnectionStatusChanged?.Invoke(e);
        }
    }
}
