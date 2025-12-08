using System;

namespace ModularSys.Helpdesk.Services
{
    public interface ISLAService
    {
        (DateTime FirstResponseDue, DateTime ResolutionDue) CalculateDueDates(string priority, DateTime createdAt);
        bool CheckFirstResponseBreach(DateTime dueAt, DateTime? actualAt);
        bool CheckResolutionBreach(DateTime dueAt, DateTime? actualAt);
    }
}
