using RetailCore.Contracts.Sales;

namespace RetailCore.Application.Abstractions;

public record CartLineInput(
    long ProductId,
    string Name,
    string Barcode,
    decimal UnitPrice,
    decimal VatPercent,
    int Quantity);

public record CartLineResult(
    long ProductId,
    string Name,
    string Barcode,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal VatAmount,
    decimal LineTotal);

public record CartCalculationResult(
    IReadOnlyList<CartLineResult> Lines,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal VatAmount,
    decimal Total,
    decimal Paid,
    decimal Change);

public interface ICartCalculationService
{
    CartCalculationResult Calculate(
        IReadOnlyList<CartLineInput> lines,
        IReadOnlyList<CheckoutPaymentRequest> payments,
        decimal cartDiscount = 0);
}
