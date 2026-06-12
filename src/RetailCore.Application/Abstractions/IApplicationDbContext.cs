using Microsoft.EntityFrameworkCore;
using RetailCore.Domain.Entities;

namespace RetailCore.Application.Abstractions;

/// <summary>Abstraction over the EF Core context so application services stay persistence-agnostic.</summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Store> Stores { get; }
    DbSet<CashRegister> CashRegisters { get; }
    DbSet<CashierShift> CashierShifts { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Receipt> Receipts { get; }
    DbSet<Refund> Refunds { get; }
    DbSet<RefundItem> RefundItems { get; }
    DbSet<DiscountRule> DiscountRules { get; }
    DbSet<Customer> Customers { get; }
    DbSet<LoyaltyAccount> LoyaltyAccounts { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
