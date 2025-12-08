using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModularSys.Core.Interfaces;

namespace ModularSys.Helpdesk.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(ILogger<EmailNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // MOCK: In a real app, use SmtpClient or SendGrid/Mailgun
            _logger.LogInformation($"[Email Notification] To: {to}, Subject: {subject}");
            _logger.LogInformation($"[Email Body]: {body}");
            
            await Task.CompletedTask;
        }

        public async Task SendTicketCreatedNotificationAsync(string toEmail, string ticketSubject, int ticketId)
        {
            var subject = $"[Ticket #{ticketId}] Received: {ticketSubject}";
            var body = $@"
Hello,

Your ticket specific to '{ticketSubject}' has been received.
Ticket ID: #{ticketId}

One of our agents will review it shortly.

Regards,
Helpdesk Team";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
