using Microsoft.EntityFrameworkCore;
using RetailCore.Application.Abstractions;
using RetailCore.Application.Common.Exceptions;
using RetailCore.Contracts.Catalog;
using RetailCore.Domain.Entities;

namespace RetailCore.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public CategoryService(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
        => await _db.Categories.OrderBy(c => c.Name).Select(c => Map(c)).ToListAsync(ct);

    public async Task<CategoryDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category", id);
        return Map(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.Name == name, ct))
        {
            throw new ConflictException($"A category named '{name}' already exists.");
        }

        var category = new Category
        {
            Name = name,
            Description = request.Description?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return Map(category);
    }

    public async Task<CategoryDto> UpdateAsync(long id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category", id);

        var name = request.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.Name == name && c.Id != id, ct))
        {
            throw new ConflictException($"A category named '{name}' already exists.");
        }

        category.Name = name;
        category.Description = request.Description?.Trim() ?? string.Empty;
        category.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return Map(category);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category", id);

        if (await _db.Products.AnyAsync(p => p.CategoryId == id, ct))
        {
            throw new ConflictException("Cannot delete a category that still has products.");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }

    private static CategoryDto Map(Category c) => new(c.Id, c.Name, c.Description, c.IsActive, c.CreatedAt);
}
