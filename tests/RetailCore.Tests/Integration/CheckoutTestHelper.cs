using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailCore.Contracts.Auth;
using RetailCore.Contracts.Sales;
using RetailCore.Contracts.Shifts;
using RetailCore.Domain.Enums;
using RetailCore.Infrastructure.Persistence;

namespace RetailCore.Tests.Integration;

internal static class CheckoutTestHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<CheckoutContext> PrepareCheckoutContextAsync(
        RetailCoreWebApplicationFactory factory,
        int stockQuantity,
        string cashierEmail = "cashier@retailcore.local")
    {
        await factory.EnsureDatabaseSeededAsync();
        await factory.ResetCheckoutStateAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RetailCoreDbContext>();
            var store = await db.Stores.FirstAsync();
            var product = await db.Products.FirstAsync();
            var inventory = await db.InventoryItems.FirstAsync(i => i.StoreId == store.Id && i.ProductId == product.Id);
            inventory.Quantity = stockQuantity;
            inventory.ReservedQuantity = 0;
            inventory.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(cashierEmail, DbSeeder.DefaultPassword));
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Login failed.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using var readScope = factory.Services.CreateScope();
        var readDb = readScope.ServiceProvider.GetRequiredService<RetailCoreDbContext>();
        var seededStore = await readDb.Stores.FirstAsync();
        var register = await readDb.CashRegisters.FirstAsync(r => r.StoreId == seededStore.Id);
        var seededProduct = await readDb.Products.FirstAsync();

        var openShift = await client.PostAsJsonAsync("/api/shifts/open", new OpenShiftRequest(register.Id, 100m));
        openShift.EnsureSuccessStatusCode();
        var shift = await openShift.Content.ReadFromJsonAsync<ShiftDto>(JsonOptions)
            ?? throw new InvalidOperationException("Open shift failed.");

        return new CheckoutContext(client, seededStore.Id, register.Id, shift.Id, seededProduct.Id, stockQuantity);
    }

    public static async Task<CheckoutContext> PrepareSecondCashierContextAsync(
        RetailCoreWebApplicationFactory factory,
        CheckoutContext first)
    {
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("manager@retailcore.local", DbSeeder.DefaultPassword));
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Login failed.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailCoreDbContext>();
        var register = await db.CashRegisters.FirstAsync(r => r.StoreId == first.StoreId);

        var openShift = await client.PostAsJsonAsync("/api/shifts/open", new OpenShiftRequest(register.Id, 50m));
        openShift.EnsureSuccessStatusCode();
        var shift = await openShift.Content.ReadFromJsonAsync<ShiftDto>(JsonOptions)
            ?? throw new InvalidOperationException("Open shift failed.");

        return first with { Client = client, ShiftId = shift.Id };
    }

    public static CheckoutRequest BuildCheckoutRequest(CheckoutContext ctx, string idempotencyKey, int quantity = 1)
        => new(
            ctx.StoreId,
            ctx.ShiftId,
            ctx.RegisterId,
            null,
            [new CheckoutItemRequest(ctx.ProductId, quantity)],
            [new CheckoutPaymentRequest(PaymentMethod.Cash, 100m)],
            idempotencyKey);

    public static async Task<int> GetStockAsync(RetailCoreWebApplicationFactory factory, long storeId, long productId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailCoreDbContext>();
        var item = await db.InventoryItems.FirstAsync(i => i.StoreId == storeId && i.ProductId == productId);
        return item.Quantity;
    }

    public static async Task<int> GetSaleCountAsync(RetailCoreWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailCoreDbContext>();
        return await db.Sales.CountAsync(s => s.SaleStatus == SaleStatus.Completed);
    }
}

internal record CheckoutContext(
    HttpClient Client,
    long StoreId,
    long RegisterId,
    long ShiftId,
    long ProductId,
    int InitialStock);
