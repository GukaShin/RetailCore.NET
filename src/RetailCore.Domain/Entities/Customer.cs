using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public LoyaltyAccount? LoyaltyAccount { get; set; }
}
