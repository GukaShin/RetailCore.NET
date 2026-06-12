namespace RetailCore.Contracts.Catalog;

public record CreateCategoryRequest(string Name, string? Description);

public record UpdateCategoryRequest(string Name, string? Description, bool IsActive);

public record CategoryDto(long Id, string Name, string Description, bool IsActive, DateTimeOffset CreatedAt);
