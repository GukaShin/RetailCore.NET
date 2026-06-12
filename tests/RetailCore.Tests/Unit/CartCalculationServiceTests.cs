using FluentAssertions;
using RetailCore.Application.Common.Exceptions;
using RetailCore.Application.Services;
using RetailCore.Contracts.Sales;
using RetailCore.Domain.Enums;

namespace RetailCore.Tests.Unit;

public class CartCalculationServiceTests
{
    private readonly CartCalculationService _sut = new();

    [Fact]
    public void Calculate_ComputesSubtotalVatTotalAndChange()
    {
        var lines = new[]
        {
            new Application.Abstractions.CartLineInput(1, "Bread", "111", 2.00m, 18m, 1),
            new Application.Abstractions.CartLineInput(2, "Milk", "222", 4.00m, 18m, 1),
            new Application.Abstractions.CartLineInput(3, "Chocolate", "333", 3.00m, 18m, 1),
        };

        var payments = new[] { new CheckoutPaymentRequest(PaymentMethod.Cash, 10m) };

        var result = _sut.Calculate(lines, payments, cartDiscount: 1m);

        result.Subtotal.Should().Be(9.00m);
        result.DiscountAmount.Should().Be(1.00m);
        result.Total.Should().Be(8.00m);
        result.Paid.Should().Be(10.00m);
        result.Change.Should().Be(2.00m);
        result.VatAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Calculate_SplitPayment_NoChange()
    {
        var lines = new[]
        {
            new Application.Abstractions.CartLineInput(1, "Item", "111", 100m, 18m, 1),
        };

        var payments = new[]
        {
            new CheckoutPaymentRequest(PaymentMethod.Cash, 40m),
            new CheckoutPaymentRequest(PaymentMethod.Card, 60m),
        };

        var result = _sut.Calculate(lines, payments);

        result.Total.Should().Be(100m);
        result.Paid.Should().Be(100m);
        result.Change.Should().Be(0m);
    }

    [Fact]
    public void Calculate_InsufficientPayment_Throws()
    {
        var lines = new[]
        {
            new Application.Abstractions.CartLineInput(1, "Item", "111", 10m, 18m, 1),
        };

        var payments = new[] { new CheckoutPaymentRequest(PaymentMethod.Cash, 5m) };

        var act = () => _sut.Calculate(lines, payments);

        act.Should().Throw<BusinessRuleException>().WithMessage("*Payment insufficient*");
    }

    [Fact]
    public void Calculate_EmptyCart_Throws()
    {
        var act = () => _sut.Calculate(Array.Empty<Application.Abstractions.CartLineInput>(), Array.Empty<CheckoutPaymentRequest>());

        act.Should().Throw<BusinessRuleException>().WithMessage("*at least one product*");
    }
}
