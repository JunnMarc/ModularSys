using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ModularSys.Data.Common.Db
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ModularSysDbContext>
    {
        public ModularSysDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(basePath, "..", "ModuERP", "appsettings.json");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(configPath, optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ModularSysDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ModularSysDbContext(optionsBuilder.Options);
        }
    }
}
