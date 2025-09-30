using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Interfaces.Sync
{
    /// <summary>
    /// Manages connections to local and cloud databases
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Gets a ModularSysDbContext connected to the local database
        /// </summary>
        ModularSysDbContext GetLocalContext();
        
        /// <summary>
        /// Gets a ModularSysDbContext connected to the cloud database
        /// </summary>
        ModularSysDbContext GetCloudContext();
        
        /// <summary>
        /// Tests connection to local database
        /// </summary>
        Task<bool> TestLocalConnectionAsync();
        
        /// <summary>
        /// Tests connection to cloud database
        /// </summary>
        Task<bool> TestCloudConnectionAsync();
        
        /// <summary>
        /// Gets the current connection mode (Local, Cloud, or Hybrid)
        /// </summary>
        ConnectionMode GetConnectionMode();
        
        /// <summary>
        /// Sets the connection mode
        /// </summary>
        void SetConnectionMode(ConnectionMode mode);
        
        /// <summary>
        /// Checks if cloud is reachable
        /// </summary>
        Task<bool> IsCloudReachableAsync();
        
        /// <summary>
        /// Gets the current cloud availability status (cached)
        /// </summary>
        bool IsCloudAvailable();
        
        /// <summary>
        /// Forces an immediate cloud connectivity check
        /// </summary>
        Task<bool> CheckCloudStatusNowAsync();
        
        /// <summary>
        /// Event fired when connection status changes
        /// </summary>
        event Action<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
    }
    
    /// <summary>
    /// Connection modes for the application
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// Only use local database (offline mode)
        /// </summary>
        Local,
        
        /// <summary>
        /// Only use cloud database (online mode)
        /// </summary>
        Cloud,
        
        /// <summary>
        /// Use local database with background sync to cloud
        /// </summary>
        Hybrid
    }
    
    /// <summary>
    /// Event args for connection status changes
    /// </summary>
    public class ConnectionStatusChangedEventArgs
    {
        public bool IsCloudAvailable { get; set; }
        public ConnectionMode ConnectionMode { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
