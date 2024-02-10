using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Equinor.ProCoSys.Completion.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<YourDbContext>
{
    public YourDbContext CreateDbContext(string[] args)
    {
        //IConfigurationRoot configuration = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile("appsettings.json")
        //    .Build();

        var builder = new DbContextOptionsBuilder<YourDbContext>();
        builder.UseSqlServer(configuration.GetConnectionString("YourConnectionStringName"));

        return new YourDbContext(builder.Options);
    }
}
