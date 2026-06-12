using Microsoft.EntityFrameworkCore;
using RetailCore.Application.Abstractions;
using RetailCore.Domain.Entities;
using RetailCore.Domain.Enums;

namespace RetailCore.Infrastructure.Persistence;

/// <summary>Seeds a baseline dataset so the system is demoable immediately after migration.</summary>
public static class DbSeeder
{
    public const string DefaultPassword = "Password123!";

    public static async Task SeedAsync(RetailCoreDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var store = new Store
        {
            Name = "RetailCore Central",
            Address = "12 Rustaveli Ave, Tbilisi",
            PhoneNumber = "+995 322 000 000",
            Status = StoreStatus.Active,
            CreatedAt = now
        };
        db.Stores.Add(store);
        await db.SaveChangesAsync(ct);

        var register = new CashRegister
        {
            StoreId = store.Id,
            Name = "Register 1",
            Code = "REG-01",
            Status = RegisterStatus.Active,
            CreatedAt = now
        };
        db.CashRegisters.Add(register);

        var hash = hasher.Hash(DefaultPassword);
        db.Users.AddRange(
            new User { FullName = "System Admin", Email = "admin@retailcore.local", PasswordHash = hash, Role = UserRole.Admin, StoreId = null, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { FullName = "Mariam Manager", Email = "manager@retailcore.local", PasswordHash = hash, Role = UserRole.StoreManager, StoreId = store.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { FullName = "Giorgi Cashier", Email = "cashier@retailcore.local", PasswordHash = hash, Role = UserRole.Cashier, StoreId = store.Id, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new User { FullName = "Nino Inventory", Email = "inventory@retailcore.local", PasswordHash = hash, Role = UserRole.InventoryManager, StoreId = store.Id, IsActive = true, CreatedAt = now, UpdatedAt = now });

        var drinks = new Category { Name = "Drinks", Description = "Soft drinks and water", IsActive = true, CreatedAt = now };
        var bakery = new Category { Name = "Bakery", Description = "Bread and pastries", IsActive = true, CreatedAt = now };
        var dairy = new Category { Name = "Dairy", Description = "Milk and dairy products", IsActive = true, CreatedAt = now };
        var snacks = new Category { Name = "Snacks", Description = "Chocolate and snacks", IsActive = true, CreatedAt = now };
        db.Categories.AddRange(drinks, bakery, dairy, snacks);
        await db.SaveChangesAsync(ct);

        var products = new List<Product>
        {
            new() { Name = "Coca-Cola 0.5L", Barcode = "4860001234567", Sku = "DRK-COLA-05", CategoryId = drinks.Id, Price = 2.50m, CostPrice = 1.70m, VatPercent = 18m, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Borjomi 0.5L", Barcode = "4860001234574", Sku = "DRK-BORJ-05", CategoryId = drinks.Id, Price = 1.80m, CostPrice = 1.10m, VatPercent = 18m, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "White Bread", Barcode = "4860002234561", Sku = "BAK-WBRD-01", CategoryId = bakery.Id, Price = 2.00m, CostPrice = 1.20m, VatPercent = 18m, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Milk 1L", Barcode = "4860003234562", Sku = "DRY-MILK-1L", CategoryId = dairy.Id, Price = 4.00m, CostPrice = 2.90m, VatPercent = 18m, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Chocolate Bar", Barcode = "4860004234563", Sku = "SNK-CHOC-01", CategoryId = snacks.Id, Price = 3.00m, CostPrice = 1.90m, VatPercent = 18m, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };
        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);

        foreach (var product in products)
        {
            db.InventoryItems.Add(new InventoryItem
            {
                StoreId = store.Id,
                ProductId = product.Id,
                Quantity = 120,
                ReservedQuantity = 0,
                LowStockThreshold = 15,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
