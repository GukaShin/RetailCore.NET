using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RetailCore.Api.Authentication;
using RetailCore.Api.Authorization;
using RetailCore.Api.Filters;
using RetailCore.Api.Middleware;
using RetailCore.Application;
using RetailCore.Application.Abstractions;
using RetailCore.Domain.Enums;
using RetailCore.Infrastructure;
using RetailCore.Infrastructure.Persistence;
using RetailCore.Infrastructure.Security;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwt = new JwtOptions
{
    Issuer = jwtSection["Issuer"] ?? new JwtOptions().Issuer,
    Audience = jwtSection["Audience"] ?? new JwtOptions().Audience,
    SigningKey = jwtSection["SigningKey"] ?? new JwtOptions().SigningKey
};

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(15)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RolePolicies.AdminOnly, p => p.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy(RolePolicies.Management, p => p.RequireRole(
        UserRole.Admin.ToString(),
        UserRole.StoreManager.ToString()));
    options.AddPolicy(RolePolicies.CatalogManagement, p => p.RequireRole(
        UserRole.Admin.ToString(),
        UserRole.StoreManager.ToString()));
    options.AddPolicy(RolePolicies.InventoryAccess, p => p.RequireRole(
        UserRole.Admin.ToString(),
        UserRole.StoreManager.ToString(),
        UserRole.InventoryManager.ToString()));
    options.AddPolicy(RolePolicies.CashierOperations, p => p.RequireRole(
        UserRole.Admin.ToString(),
        UserRole.StoreManager.ToString(),
        UserRole.Cashier.ToString()));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:5173", "http://localhost:5174", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RetailCore.NET API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT access token (without the 'Bearer ' prefix).",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await InitializeDatabaseAsync(app);
}

app.UseSerilogRequestLogging();
app.UseMiddleware<RequestCounterMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RetailCoreDbContext>();
    await db.Database.MigrateAsync();

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(db, hasher);
}

public partial class Program;

