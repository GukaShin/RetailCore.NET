using RetailCore.Domain.Common;
using RetailCore.Domain.Enums;

namespace RetailCore.Domain.Entities;

public class Refund : BaseEntity
{
    public long SaleId { get; set; }
    public Sale? Sale { get; set; }

    public long CashierId { get; set; }
    public User? Cashier { get; set; }

    public string Reason { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<RefundItem> Items { get; set; } = new List<RefundItem>();
}
