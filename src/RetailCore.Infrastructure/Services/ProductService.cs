using Microsoft.EntityFrameworkCore;
using RetailCore.Application.Abstractions;
using RetailCore.Application.Common.Exceptions;
using RetailCore.Contracts.Catalog;
using RetailCore.Domain.Entities;

namespace RetailCore.Infrastructure.Services;

public class ProductService : IProductService
{
    private static readonly TimeSpan BarcodeCacheTtl = TimeSpan.FromMinutes(30);

    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;
    private readonly IDateTimeProvider _clock;

    public ProductService(IApplicationDbContext db, ICacheService cache, IDateTimeProvider clock)
    {
        _db = db;
        _cache = cache;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ProductDto>> SearchAsync(string? search, long? categoryId, CancellationToken ct = default)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (categoryId is { } cat)
        {
            query = query.Where(p => p.CategoryId == cat);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%") ||
                EF.Functions.ILike(p.Barcode, $"%{term}%") ||
                EF.Functions.ILike(p.Sku, $"%{term}%"));
        }

        return await query.OrderBy(p => p.Name).Select(p => Map(p)).ToListAsync(ct);
    }

    public async Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Product", id);
        return Map(product);
    }

    public async Task<ProductDto> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        barcode = barcode.Trim();
        var cacheKey = CacheKeys.ProductByBarcode(barcode);

        var cached = await _cache.GetAsync<ProductDto>(cacheKey, ct);
        if (cached is not null)
        {
            return cached;
        }

        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Barcode == barcode, ct)
            ?? throw new NotFoundException($"No product found with barcode '{barcode}'.");

        var dto = Map(product);
        await _cache.SetAsync(cacheKey, dto, BarcodeCacheTtl, ct);
        return dto;
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        await EnsureCategoryExists(request.CategoryId, ct);
        await EnsureUnique(request.Barcode, request.Sku, null, ct);

        var now = _clock.UtcNow;
        var product = new Product
        {
            Name = request.Name.Trim(),
            Barcode = request.Barcode.Trim(),
            Sku = request.Sku.Trim(),
            CategoryId = request.CategoryId,
            Price = request.Price,
            CostPrice = request.CostPrice,
            VatPercent = request.VatPercent,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(product.Id, ct);
    }

    public async Task<ProductDto> UpdateAsync(long id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Product", id);

        await EnsureCategoryExists(request.CategoryId, ct);
        await EnsureUnique(request.Barcode, request.Sku, id, ct);

        var oldBarcode = product.Barcode;

        product.Name = request.Name.Trim();
        product.Barcode = request.Barcode.Trim();
        product.Sku = request.Sku.Trim();
        product.CategoryId = request.CategoryId;
        product.Price = request.Price;
        product.CostPrice = request.CostPrice;
        product.VatPercent = request.VatPercent;
        product.IsActive = request.IsActive;
        product.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Invalidate any cached barcode lookups (old and new) so stale prices are never served.
        await _cache.RemoveAsync(CacheKeys.ProductByBarcode(oldBarcode), ct);
        await _cache.RemoveAsync(CacheKeys.ProductByBarcode(product.Barcode), ct);

        return await GetByIdAsync(product.Id, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Product", id);

        // Soft delete to preserve sales history; barcode cache is invalidated.
        product.IsActive = false;
        product.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.ProductByBarcode(product.Barcode), ct);
    }

    private async Task EnsureCategoryExists(long categoryId, CancellationToken ct)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == categoryId, ct))
        {
            throw new NotFoundException("Category", categoryId);
        }
    }

    private async Task EnsureUnique(string barcode, string sku, long? excludeId, CancellationToken ct)
    {
        barcode = barcode.Trim();
        sku = sku.Trim();
        if (await _db.Products.AnyAsync(p => p.Barcode == barcode && (excludeId == null || p.Id != excludeId), ct))
        {
            throw new ConflictException($"A product with barcode '{barcode}' already exists.");
        }
        if (await _db.Products.AnyAsync(p => p.Sku == sku && (excludeId == null || p.Id != excludeId), ct))
        {
            throw new ConflictException($"A product with SKU '{sku}' already exists.");
        }
    }

    private static ProductDto Map(Product p) => new(
        p.Id,
        p.Name,
        p.Barcode,
        p.Sku,
        p.CategoryId,
        p.Category?.Name ?? string.Empty,
        p.Price,
        p.CostPrice,
        p.VatPercent,
        p.IsActive);
}
