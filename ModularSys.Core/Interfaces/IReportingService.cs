using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModularSys.Core.Interfaces
{
    public interface IReportingService
    {
        Task<Dictionary<string, double>> GetCustomerSatisfactionStatsAsync();
        Task<Dictionary<string, int>> GetTicketsByChannelStatsAsync();
        Task<double> GetAverageResolutionTimeAsync();
        Task<int> GetSLABreachCountAsync();
        Task<Dictionary<DateTime, int>> GetTicketVolumeByDayAsync(int days);
        Task<Dictionary<string, int>> GetTicketVolumeByPriorityAsync();
    }
}
