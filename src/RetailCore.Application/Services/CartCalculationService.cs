using RetailCore.Application.Abstractions;
using RetailCore.Application.Common.Exceptions;
using RetailCore.Contracts.Sales;
using RetailCore.Domain.Enums;

namespace RetailCore.Application.Services;

/// <summary>
/// Pure cart math: VAT-inclusive prices, line totals, payment validation, and cash change.
/// Kept free of persistence so it can be unit-tested in isolation.
/// </summary>
public class CartCalculationService : ICartCalculationService
{
    public CartCalculationResult Calculate(
        IReadOnlyList<CartLineInput> lines,
        IReadOnlyList<CheckoutPaymentRequest> payments,
        decimal cartDiscount = 0)
    {
        if (lines.Count == 0)
        {
            throw new BusinessRuleException("A sale must contain at least one product.");
        }

        if (payments.Count == 0)
        {
            throw new BusinessRuleException("At least one payment is required.");
        }

        var lineResults = new List<CartLineResult>();
        decimal subtotal = 0;
        decimal totalVat = 0;

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
            {
                throw new BusinessRuleException($"Quantity must be positive for product {line.ProductId}.");
            }

            // Price is VAT-inclusive; extract VAT component from the gross line total.
            var gross = line.UnitPrice * line.Quantity;
            var vatRate = line.VatPercent / 100m;
            var vat = vatRate > 0 ? gross - gross / (1 + vatRate) : 0m;
            vat = Math.Round(vat, 2, MidpointRounding.AwayFromZero);

            var lineTotal = Math.Round(gross, 2, MidpointRounding.AwayFromZero);
            subtotal += lineTotal;
            totalVat += vat;

            lineResults.Add(new CartLineResult(
                line.ProductId,
                line.Name,
                line.Barcode,
                line.Quantity,
                line.UnitPrice,
                0,
                vat,
                lineTotal));
        }

        subtotal = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero);
        cartDiscount = Math.Round(Math.Max(0, cartDiscount), 2, MidpointRounding.AwayFromZero);

        if (cartDiscount > subtotal)
        {
            throw new BusinessRuleException("Cart discount cannot exceed subtotal.");
        }

        var total = Math.Round(subtotal - cartDiscount, 2, MidpointRounding.AwayFromZero);
        totalVat = Math.Round(totalVat, 2, MidpointRounding.AwayFromZero);

        var paid = Math.Round(payments.Sum(p => p.Amount), 2, MidpointRounding.AwayFromZero);
        if (paid < total)
        {
            throw new BusinessRuleException($"Payment insufficient: paid {paid}, total {total}.");
        }

        var nonCashPaid = payments
            .Where(p => p.PaymentMethod != PaymentMethod.Cash)
            .Sum(p => p.Amount);

        var cashPaid = payments
            .Where(p => p.PaymentMethod == PaymentMethod.Cash)
            .Sum(p => p.Amount);

        var amountDueAfterNonCash = Math.Max(0, total - nonCashPaid);
        var change = cashPaid > amountDueAfterNonCash
            ? Math.Round(cashPaid - amountDueAfterNonCash, 2, MidpointRounding.AwayFromZero)
            : 0m;

        return new CartCalculationResult(lineResults, subtotal, cartDiscount, totalVat, total, paid, change);
    }
}
