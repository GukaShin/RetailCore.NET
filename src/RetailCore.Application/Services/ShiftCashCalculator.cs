namespace RetailCore.Application.Services;

/// <summary>Pure shift close cash math, separated for unit testing.</summary>
public static class ShiftCashCalculator
{
    public static decimal CalculateExpectedCash(decimal openingCash, decimal cashSales, decimal cashRefunds)
        => Math.Round(openingCash + cashSales - cashRefunds, 2, MidpointRounding.AwayFromZero);

    public static decimal CalculateDifference(decimal actualCash, decimal expectedCash)
        => Math.Round(actualCash - expectedCash, 2, MidpointRounding.AwayFromZero);
}
