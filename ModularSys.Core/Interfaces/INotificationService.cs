using System.Threading.Tasks;

namespace ModularSys.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendTicketCreatedNotificationAsync(string toEmail, string ticketSubject, int ticketId);
    }
}
