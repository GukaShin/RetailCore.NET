using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCore.Api.Authorization;
using RetailCore.Application.Abstractions;
using RetailCore.Contracts.Catalog;

namespace RetailCore.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = RolePolicies.CatalogManagement)]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories) => _categories = categories;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct)
        => Ok(await _categories.GetAllAsync(ct));

    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetById(long id, CancellationToken ct)
        => Ok(await _categories.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
        => Ok(await _categories.CreateAsync(request, ct));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CategoryDto>> Update(long id, UpdateCategoryRequest request, CancellationToken ct)
        => Ok(await _categories.UpdateAsync(id, request, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _categories.DeleteAsync(id, ct);
        return NoContent();
    }
}
