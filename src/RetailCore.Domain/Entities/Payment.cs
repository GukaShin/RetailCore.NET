using RetailCore.Domain.Common;
using RetailCore.Domain.Enums;

namespace RetailCore.Domain.Entities;

public class Payment : BaseEntity
{
    public long SaleId { get; set; }
    public Sale? Sale { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Amount tendered for this payment line. For cash this may exceed the line's share (change is returned).</summary>
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ReferenceNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}
