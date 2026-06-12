namespace RetailCore.Contracts.Catalog;

public record CreateProductRequest(
    string Name,
    string Barcode,
    string Sku,
    long CategoryId,
    decimal Price,
    decimal CostPrice,
    decimal VatPercent);

public record UpdateProductRequest(
    string Name,
    string Barcode,
    string Sku,
    long CategoryId,
    decimal Price,
    decimal CostPrice,
    decimal VatPercent,
    bool IsActive);

public record ProductDto(
    long Id,
    string Name,
    string Barcode,
    string Sku,
    long CategoryId,
    string CategoryName,
    decimal Price,
    decimal CostPrice,
    decimal VatPercent,
    bool IsActive);
