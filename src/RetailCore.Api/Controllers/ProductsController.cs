using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCore.Api.Authorization;
using RetailCore.Application.Abstractions;
using RetailCore.Contracts.Catalog;

namespace RetailCore.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;

    public ProductsController(IProductService products) => _products = products;

    [HttpGet]
    [Authorize(Policy = RolePolicies.CatalogManagement)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Search(
        [FromQuery] string? search,
        [FromQuery] long? categoryId,
        CancellationToken ct)
        => Ok(await _products.SearchAsync(search, categoryId, ct));

    [HttpGet("{id:long}")]
    [Authorize(Policy = RolePolicies.CashierOperations)]
    public async Task<ActionResult<ProductDto>> GetById(long id, CancellationToken ct)
        => Ok(await _products.GetByIdAsync(id, ct));

    [HttpGet("barcode/{barcode}")]
    [Authorize(Policy = RolePolicies.CashierOperations)]
    public async Task<ActionResult<ProductDto>> GetByBarcode(string barcode, CancellationToken ct)
        => Ok(await _products.GetByBarcodeAsync(barcode, ct));

    [HttpPost]
    [Authorize(Policy = RolePolicies.CatalogManagement)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
        => Ok(await _products.CreateAsync(request, ct));

    [HttpPut("{id:long}")]
    [Authorize(Policy = RolePolicies.CatalogManagement)]
    public async Task<ActionResult<ProductDto>> Update(long id, UpdateProductRequest request, CancellationToken ct)
        => Ok(await _products.UpdateAsync(id, request, ct));

    [HttpDelete("{id:long}")]
    [Authorize(Policy = RolePolicies.CatalogManagement)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _products.DeleteAsync(id, ct);
        return NoContent();
    }
}
