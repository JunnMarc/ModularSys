using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModularSys.Data.Common.Services.Sync
{
    /// <summary>
    /// Provides retry logic with exponential backoff for sync operations
    /// </summary>
    public class RetryService
    {
        private readonly ILogger<RetryService> _logger;

        public RetryService(ILogger<RetryService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes an async operation with retry logic and exponential backoff
        /// </summary>
        public async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 3,
            int initialDelaySeconds = 5,
            bool useExponentialBackoff = true,
            CancellationToken cancellationToken = default)
        {
            var attempt = 0;
            var delay = TimeSpan.FromSeconds(initialDelaySeconds);

            while (true)
            {
                attempt++;
                
                try
                {
                    _logger.LogDebug("Executing operation, attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                    return await operation();
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(ex, 
                        "Operation failed on attempt {Attempt}/{MaxRetries}. Retrying in {Delay}...", 
                        attempt, maxRetries, delay);

                    await Task.Delay(delay, cancellationToken);

                    // Calculate next delay with exponential backoff
                    if (useExponentialBackoff)
                    {
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Operation failed after {Attempt} attempts. Giving up.", attempt);
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes an async operation with retry logic (void return)
        /// </summary>
        public async Task ExecuteWithRetryAsync(
            Func<Task> operation,
            int maxRetries = 3,
            int initialDelaySeconds = 5,
            bool useExponentialBackoff = true,
            CancellationToken cancellationToken = default)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await operation();
                return true;
            }, maxRetries, initialDelaySeconds, useExponentialBackoff, cancellationToken);
        }

        /// <summary>
        /// Calculates the next retry time based on retry count and exponential backoff
        /// </summary>
        public DateTime CalculateNextRetryTime(int retryCount, int initialDelaySeconds = 5, bool useExponentialBackoff = true)
        {
            var delay = initialDelaySeconds;
            
            if (useExponentialBackoff)
            {
                delay = (int)(initialDelaySeconds * Math.Pow(2, retryCount));
            }
            else
            {
                delay = initialDelaySeconds * (retryCount + 1);
            }

            // Cap maximum delay at 1 hour
            delay = Math.Min(delay, 3600);

            return DateTime.UtcNow.AddSeconds(delay);
        }

        /// <summary>
        /// Checks if it's time to retry based on NextRetryAt timestamp
        /// </summary>
        public bool ShouldRetry(DateTime? nextRetryAt)
        {
            if (!nextRetryAt.HasValue)
            {
                return true;
            }

            return DateTime.UtcNow >= nextRetryAt.Value;
        }
    }
}
