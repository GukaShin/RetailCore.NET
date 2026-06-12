using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class RefundItem : BaseEntity
{
    public long RefundId { get; set; }
    public Refund? Refund { get; set; }

    public long SaleItemId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal RefundAmount { get; set; }
}
