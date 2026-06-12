using RetailCore.Contracts.Catalog;
using RetailCore.Contracts.Inventory;

namespace RetailCore.Application.Abstractions;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(long id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> SearchAsync(string? search, long? categoryId, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ProductDto> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(long id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItemDto>> GetByStoreAsync(long storeId, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryItemDto>> GetLowStockAsync(long storeId, CancellationToken ct = default);
    Task<InventoryItemDto> AdjustAsync(long storeId, StockAdjustRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ProductStockDto>> GetProductStockAsync(long productId, CancellationToken ct = default);
}
