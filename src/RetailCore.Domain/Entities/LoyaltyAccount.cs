using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class LoyaltyAccount : BaseEntity
{
    public long CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int Points { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
