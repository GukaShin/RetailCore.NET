using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RetailCore.Application.Abstractions;
using RetailCore.Domain.Entities;

namespace RetailCore.Infrastructure.Persistence;

public class RetailCoreDbContext : DbContext, IApplicationDbContext
{
    public RetailCoreDbContext(DbContextOptions<RetailCoreDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<RefundItem> RefundItems => Set<RefundItem>();
    public DbSet<DiscountRule> DiscountRules => Set<DiscountRule>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailCoreDbContext).Assembly);

        // Persist all enums as readable strings rather than integers.
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                var underlying = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (!underlying.IsEnum)
                {
                    continue;
                }

                var converterType = typeof(EnumToStringConverter<>).MakeGenericType(underlying);
                var converter = (ValueConverter)Activator.CreateInstance(converterType, new object?[] { null })!;
                property.SetValueConverter(converter);
                property.SetMaxLength(40);
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HaveColumnType("numeric(18,2)");
        configurationBuilder.Properties<string>().HaveMaxLength(512);
    }
}
