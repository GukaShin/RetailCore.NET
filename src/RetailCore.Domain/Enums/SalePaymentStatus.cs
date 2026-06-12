namespace RetailCore.Domain.Enums;

/// <summary>Aggregate payment state of a <see cref="Entities.Sale"/>.</summary>
public enum SalePaymentStatus
{
    Pending,
    Paid,
    PartiallyPaid,
    Failed,
    Refunded
}
