using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;

    public long CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>Retail price. Treated as VAT-inclusive.</summary>
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }

    /// <summary>VAT rate as a percentage, e.g. 18 for 18%.</summary>
    public decimal VatPercent { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
