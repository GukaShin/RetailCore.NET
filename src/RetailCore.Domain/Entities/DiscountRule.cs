using RetailCore.Domain.Common;
using RetailCore.Domain.Enums;

namespace RetailCore.Domain.Entities;

public class DiscountRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }

    /// <summary>Percentage (0-100) or fixed amount depending on <see cref="DiscountType"/>.</summary>
    public decimal Value { get; set; }

    public long? CategoryId { get; set; }
    public long? ProductId { get; set; }
    public int? MinimumQuantity { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}
