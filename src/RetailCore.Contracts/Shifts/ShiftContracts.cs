namespace RetailCore.Contracts.Shifts;

public record OpenShiftRequest(long CashRegisterId, decimal OpeningCashAmount);

public record CloseShiftRequest(decimal ActualCashAmount);

public record ShiftDto(
    long Id,
    long CashierId,
    long StoreId,
    long CashRegisterId,
    decimal OpeningCashAmount,
    decimal? ExpectedCashAmount,
    decimal? ActualCashAmount,
    decimal? DifferenceAmount,
    string Status,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt);
