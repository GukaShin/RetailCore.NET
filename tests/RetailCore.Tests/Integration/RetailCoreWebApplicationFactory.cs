using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetailCore.Application.Abstractions;
using RetailCore.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RetailCore.Tests.Integration;

public class RetailCoreWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("retailcore_test")
        .WithUsername("retailcore")
        .WithPassword("retailcore")
        .Build();

    private bool _initialized;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await EnsureDatabaseSeededAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RetailCoreDbContext>>();
            services.RemoveAll<RetailCoreDbContext>();
            services.RemoveAll<IApplicationDbContext>();

            services.AddDbContext<RetailCoreDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<RetailCoreDbContext>());
        });
    }

    public async Task EnsureDatabaseSeededAsync()
    {
        if (_initialized)
        {
            return;
        }

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailCoreDbContext>();
        await db.Database.MigrateAsync();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DbSeeder.SeedAsync(db, hasher);
        _initialized = true;
    }

    public async Task ResetCheckoutStateAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailCoreDbContext>();

        await db.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE
                idempotency_records,
                audit_logs,
                stock_movements,
                payments,
                sale_items,
                receipts,
                sales,
                cashier_shifts
            RESTART IDENTITY CASCADE
            """);

        // Restore baseline inventory after truncating movements/sales.
        var items = await db.InventoryItems.ToListAsync();
        foreach (var item in items)
        {
            item.Quantity = 120;
            item.ReservedQuantity = 0;
        }

        await db.SaveChangesAsync();
    }
}
