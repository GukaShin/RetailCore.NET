using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class Receipt : BaseEntity
{
    public long SaleId { get; set; }
    public Sale? Sale { get; set; }

    public string ReceiptNumber { get; set; } = string.Empty;
    public string ReceiptText { get; set; } = string.Empty;

    /// <summary>Path to a generated PDF, populated asynchronously by the worker (later phase).</summary>
    public string? PdfPath { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
