using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RetailCore.Infrastructure.Persistence;

/// <summary>
/// Used by the EF Core CLI tools (migrations) at design time. The connection string here
/// only needs to be valid enough to build the model; it is not used at runtime.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RetailCoreDbContext>
{
    public RetailCoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("RETAILCORE_DB")
            ?? "Host=localhost;Port=5432;Database=retailcore;Username=retailcore;Password=retailcore";

        var options = new DbContextOptionsBuilder<RetailCoreDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new RetailCoreDbContext(options);
    }
}
