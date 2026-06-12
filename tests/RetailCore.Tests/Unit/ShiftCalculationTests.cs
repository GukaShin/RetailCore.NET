using FluentAssertions;
using RetailCore.Application.Services;

namespace RetailCore.Tests.Unit;

public class ShiftCalculationTests
{
    [Fact]
    public void CalculateExpectedCash_IncludesOpeningCashSalesAndRefunds()
    {
        var expected = ShiftCashCalculator.CalculateExpectedCash(openingCash: 100m, cashSales: 450m, cashRefunds: 30m);

        expected.Should().Be(520m);
    }

    [Fact]
    public void CalculateExpectedCash_ShortageScenario()
    {
        var expected = ShiftCashCalculator.CalculateExpectedCash(openingCash: 100m, cashSales: 450m, cashRefunds: 30m);
        var difference = ShiftCashCalculator.CalculateDifference(actualCash: 515m, expected);

        difference.Should().Be(-5m);
    }
}
