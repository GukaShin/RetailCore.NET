namespace RetailCore.Domain.Enums;

/// <summary>State of an individual <see cref="Entities.Payment"/> line.</summary>
public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Cancelled,
    Refunded
}
