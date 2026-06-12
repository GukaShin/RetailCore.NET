using RetailCore.Domain.Common;
using RetailCore.Domain.Enums;

namespace RetailCore.Domain.Entities;

public class Sale : BaseEntity
{
    public long StoreId { get; set; }
    public Store? Store { get; set; }

    public long CashierId { get; set; }
    public User? Cashier { get; set; }

    public long ShiftId { get; set; }
    public CashierShift? Shift { get; set; }

    public long CashRegisterId { get; set; }

    public long? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string ReceiptNumber { get; set; } = string.Empty;

    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public SalePaymentStatus PaymentStatus { get; set; } = SalePaymentStatus.Pending;
    public SaleStatus SaleStatus { get; set; } = SaleStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Receipt? Receipt { get; set; }
}
