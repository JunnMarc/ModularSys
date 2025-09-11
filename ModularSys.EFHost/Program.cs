using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularSys.Data.Common.Db; // Namespace where InventoryDbContext lives

namespace ModularSys.EFHost
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create a minimal host so EF Core CLI can find the DbContext
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

                    services.AddDbContext<InventoryDbContext>(options =>
                        options.UseSqlServer(connectionString));
                })
                .Build()
                .Run();
        }
    }
}
