using Microsoft.EntityFrameworkCore;
using RetailCore.Application.Abstractions;
using RetailCore.Application.Common.Exceptions;
using RetailCore.Contracts.Inventory;
using RetailCore.Domain.Entities;
using RetailCore.Domain.Enums;

namespace RetailCore.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private static readonly TimeSpan LowStockCacheTtl = TimeSpan.FromMinutes(2);

    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUser _currentUser;

    public InventoryService(IApplicationDbContext db, ICacheService cache, IDateTimeProvider clock, ICurrentUser currentUser)
    {
        _db = db;
        _cache = cache;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetByStoreAsync(long storeId, CancellationToken ct = default)
    {
        await EnsureStoreExists(storeId, ct);

        return await _db.InventoryItems
            .Where(i => i.StoreId == storeId)
            .Include(i => i.Product)
            .OrderBy(i => i.Product!.Name)
            .Select(i => Map(i))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetLowStockAsync(long storeId, CancellationToken ct = default)
    {
        await EnsureStoreExists(storeId, ct);

        var cacheKey = CacheKeys.LowStock(storeId);
        var cached = await _cache.GetAsync<List<InventoryItemDto>>(cacheKey, ct);
        if (cached is not null)
        {
            return cached;
        }

        var items = await _db.InventoryItems
            .Where(i => i.StoreId == storeId && i.Quantity - i.ReservedQuantity <= i.LowStockThreshold)
            .Include(i => i.Product)
            .OrderBy(i => i.Quantity)
            .Select(i => Map(i))
            .ToListAsync(ct);

        await _cache.SetAsync(cacheKey, items, LowStockCacheTtl, ct);
        return items;
    }

    public async Task<InventoryItemDto> AdjustAsync(long storeId, StockAdjustRequest request, CancellationToken ct = default)
    {
        await EnsureStoreExists(storeId, ct);

        if (request.QuantityChange == 0)
        {
            throw new BusinessRuleException("Quantity change must not be zero.");
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var item = await _db.InventoryItems
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.ProductId == request.ProductId, ct);

        if (item is null)
        {
            item = new InventoryItem
            {
                StoreId = storeId,
                ProductId = request.ProductId,
                Quantity = 0,
                ReservedQuantity = 0,
                LowStockThreshold = 10,
                UpdatedAt = _clock.UtcNow,
                Product = product
            };
            _db.InventoryItems.Add(item);
        }

        var newQuantity = item.Quantity + request.QuantityChange;
        if (newQuantity < 0)
        {
            throw new BusinessRuleException(
                $"Adjustment would make stock negative (current {item.Quantity}, change {request.QuantityChange}).");
        }

        item.Quantity = newQuantity;
        item.UpdatedAt = _clock.UtcNow;

        _db.StockMovements.Add(new StockMovement
        {
            StoreId = storeId,
            ProductId = request.ProductId,
            QuantityChange = request.QuantityChange,
            MovementType = StockMovementType.ManualAdjustment,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? "Manual adjustment" : request.Reason.Trim(),
            CreatedByUserId = _currentUser.UserId ?? 0,
            CreatedAt = _clock.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.LowStock(storeId), ct);

        return Map(item);
    }

    public async Task<IReadOnlyList<ProductStockDto>> GetProductStockAsync(long productId, CancellationToken ct = default)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == productId, ct))
        {
            throw new NotFoundException("Product", productId);
        }

        return await _db.InventoryItems
            .Where(i => i.ProductId == productId)
            .Include(i => i.Store)
            .Select(i => new ProductStockDto(
                i.StoreId,
                i.Store!.Name,
                i.Quantity,
                i.Quantity - i.ReservedQuantity))
            .ToListAsync(ct);
    }

    private async Task EnsureStoreExists(long storeId, CancellationToken ct)
    {
        if (!await _db.Stores.AnyAsync(s => s.Id == storeId, ct))
        {
            throw new NotFoundException("Store", storeId);
        }
    }

    private static InventoryItemDto Map(InventoryItem i) => new(
        i.ProductId,
        i.Product?.Name ?? string.Empty,
        i.Product?.Barcode ?? string.Empty,
        i.Quantity,
        i.ReservedQuantity,
        i.Quantity - i.ReservedQuantity,
        i.LowStockThreshold,
        i.Quantity - i.ReservedQuantity <= i.LowStockThreshold);
}
