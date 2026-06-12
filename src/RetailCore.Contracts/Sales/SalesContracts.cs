using RetailCore.Domain.Enums;

namespace RetailCore.Contracts.Sales;

public record CheckoutItemRequest(long ProductId, int Quantity);

public record CheckoutPaymentRequest(PaymentMethod PaymentMethod, decimal Amount);

public record CheckoutRequest(
    long StoreId,
    long ShiftId,
    long CashRegisterId,
    long? CustomerId,
    IReadOnlyList<CheckoutItemRequest> Items,
    IReadOnlyList<CheckoutPaymentRequest> Payments,
    string IdempotencyKey);

public record CheckoutResponse(
    long SaleId,
    string ReceiptNumber,
    decimal Subtotal,
    decimal Discount,
    decimal Vat,
    decimal Total,
    decimal Paid,
    decimal Change,
    string Status);

public record SaleDto(
    long Id,
    string ReceiptNumber,
    long StoreId,
    long CashierId,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal VatAmount,
    decimal TotalAmount,
    string PaymentStatus,
    string SaleStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public record SaleItemDto(
    long Id,
    long ProductId,
    string ProductName,
    string Barcode,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal VatAmount,
    decimal LineTotal);

public record SaleDetailDto(
    long Id,
    string ReceiptNumber,
    long StoreId,
    long CashierId,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal VatAmount,
    decimal TotalAmount,
    string PaymentStatus,
    string SaleStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<SaleItemDto> Items,
    IReadOnlyList<PaymentDto> Payments);

public record PaymentDto(
    long Id,
    string PaymentMethod,
    decimal Amount,
    string Status,
    DateTimeOffset CreatedAt);

public record ReceiptDto(
    long Id,
    long SaleId,
    string ReceiptNumber,
    string ReceiptText,
    DateTimeOffset CreatedAt);
