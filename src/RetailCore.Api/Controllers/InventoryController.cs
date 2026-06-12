using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCore.Api.Authorization;
using RetailCore.Application.Abstractions;
using RetailCore.Contracts.Inventory;

namespace RetailCore.Api.Controllers;

[ApiController]
[Authorize(Policy = RolePolicies.InventoryAccess)]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventory;

    public InventoryController(IInventoryService inventory) => _inventory = inventory;

    [HttpGet("api/stores/{storeId:long}/inventory")]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> GetByStore(long storeId, CancellationToken ct)
        => Ok(await _inventory.GetByStoreAsync(storeId, ct));

    [HttpGet("api/stores/{storeId:long}/inventory/low-stock")]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> GetLowStock(long storeId, CancellationToken ct)
        => Ok(await _inventory.GetLowStockAsync(storeId, ct));

    [HttpPost("api/stores/{storeId:long}/inventory/adjust")]
    public async Task<ActionResult<InventoryItemDto>> Adjust(long storeId, StockAdjustRequest request, CancellationToken ct)
        => Ok(await _inventory.AdjustAsync(storeId, request, ct));

    [HttpGet("api/products/{productId:long}/stock")]
    public async Task<ActionResult<IReadOnlyList<ProductStockDto>>> GetProductStock(long productId, CancellationToken ct)
        => Ok(await _inventory.GetProductStockAsync(productId, ct));
}
