using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetailCore.Application.Abstractions;
using RetailCore.Infrastructure.Caching;
using RetailCore.Infrastructure.Common;
using RetailCore.Infrastructure.Persistence;
using RetailCore.Infrastructure.Security;
using RetailCore.Infrastructure.Services;
using StackExchange.Redis;

namespace RetailCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Postgres"]
            ?? "Host=localhost;Port=5432;Database=retailcore;Username=retailcore;Password=retailcore";

        services.AddDbContext<RetailCoreDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<RetailCoreDbContext>());

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        BindJwtOptions(services, configuration);
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        var redisConnection = configuration["ConnectionStrings:Redis"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<ISaleService, SaleService>();

        return services;
    }

    private static void BindJwtOptions(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = section["Issuer"] ?? options.Issuer;
            options.Audience = section["Audience"] ?? options.Audience;
            options.SigningKey = section["SigningKey"] ?? options.SigningKey;
            if (int.TryParse(section["AccessTokenMinutes"], out var minutes))
            {
                options.AccessTokenMinutes = minutes;
            }
            if (int.TryParse(section["RefreshTokenDays"], out var days))
            {
                options.RefreshTokenDays = days;
            }
        });
    }
}
