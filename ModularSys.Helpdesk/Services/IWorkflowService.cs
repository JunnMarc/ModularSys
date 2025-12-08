using System.Threading.Tasks;
using ModularSys.Data.Common.Entities.Helpdesk;

namespace ModularSys.Helpdesk.Services
{
    public interface IWorkflowService
    {
        Task ProcessTicketCreationAsync(Ticket ticket);
    }
}
