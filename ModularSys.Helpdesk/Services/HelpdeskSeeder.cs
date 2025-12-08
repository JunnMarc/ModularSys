using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities.CRM;
using ModularSys.Data.Common.Entities.Helpdesk;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModularSys.Helpdesk.Services
{
    public class HelpdeskSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ModularSysDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<HelpdeskSeeder>>();

            try
            {
                if (!context.Customers.Any())
                {
                    logger.LogInformation("Seeding Customers...");
                    context.Customers.AddRange(
                        new Customer { CompanyName = "Ayala Land", ContactName = "Juan dela Cruz", Email = "juan@ayala.com", City = "Makati", Country = "Philippines", Status = "Customer", LoyaltyPoints = 1500, SatisfactionScore = 4.8 },
                        new Customer { CompanyName = "SM Prime", ContactName = "Maria Clara", Email = "maria@sm.com", City = "Pasay", Country = "Philippines", Status = "Customer", LoyaltyPoints = 2000, SatisfactionScore = 4.5 },
                        new Customer { CompanyName = "Jollibee Foods", ContactName = "Tony Tan", Email = "tony@jollibee.com", City = "Pasig", Country = "Philippines", Status = "VIP", LoyaltyPoints = 5000, SatisfactionScore = 5.0 }
                    );
                    await context.SaveChangesAsync();
                }

                if (!context.Tickets.Any())
                {
                    logger.LogInformation("Seeding Tickets...");
                    var customer = context.Customers.FirstOrDefault();
                    if (customer != null)
                    {
                        context.Tickets.AddRange(
                            new Ticket { Subject = "Internet connectivity issue in BGC office", Status = "Open", Priority = "High", Channel = "Phone", CustomerId = customer.Id, CreatedAt = DateTime.UtcNow.AddHours(-2), Description = "User reporting slow connection." },
                            new Ticket { Subject = "Printer not working", Status = "Resolved", Priority = "Medium", Channel = "Email", CustomerId = customer.Id, CreatedAt = DateTime.UtcNow.AddDays(-1), ResolvedAt = DateTime.UtcNow, Description = "Paper jam resolved." }
                        );
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding helpdesk data");
            }
        }
    }
}
