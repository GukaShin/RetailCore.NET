using RetailCore.Domain.Common;
using RetailCore.Domain.Enums;

namespace RetailCore.Domain.Entities;

public class CashierShift : BaseEntity
{
    public long CashierId { get; set; }
    public User? Cashier { get; set; }

    public long StoreId { get; set; }
    public Store? Store { get; set; }

    public long CashRegisterId { get; set; }
    public CashRegister? CashRegister { get; set; }

    public decimal OpeningCashAmount { get; set; }

    /// <summary>Computed at close: opening + cash sales - cash refunds.</summary>
    public decimal? ExpectedCashAmount { get; set; }

    /// <summary>Cash counted by the cashier at close.</summary>
    public decimal? ActualCashAmount { get; set; }

    /// <summary>ActualCashAmount - ExpectedCashAmount (negative = shortage).</summary>
    public decimal? DifferenceAmount { get; set; }

    public ShiftStatus Status { get; set; } = ShiftStatus.Open;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
}
