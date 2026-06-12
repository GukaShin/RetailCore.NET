using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class SaleItem : BaseEntity
{
    public long SaleId { get; set; }
    public Sale? Sale { get; set; }

    public long ProductId { get; set; }

    /// <summary>Snapshot of product name at sale time so receipts stay correct if the product changes later.</summary>
    public string ProductNameSnapshot { get; set; } = string.Empty;

    /// <summary>Snapshot of barcode at sale time.</summary>
    public string BarcodeSnapshot { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal LineTotal { get; set; }
}
