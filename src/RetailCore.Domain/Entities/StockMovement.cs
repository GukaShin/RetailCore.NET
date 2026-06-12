using RetailCore.Domain.Common;
using RetailCore.Domain.Enums;

namespace RetailCore.Domain.Entities;

/// <summary>Immutable audit record of every change to inventory quantity.</summary>
public class StockMovement : BaseEntity
{
    public long StoreId { get; set; }
    public long ProductId { get; set; }

    /// <summary>Signed delta applied to stock (negative for sales, positive for restock/refund).</summary>
    public int QuantityChange { get; set; }

    public StockMovementType MovementType { get; set; }
    public string Reason { get; set; } = string.Empty;

    /// <summary>Id of the related aggregate (e.g. SaleId, RefundId), if any.</summary>
    public long? ReferenceId { get; set; }

    public long CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
