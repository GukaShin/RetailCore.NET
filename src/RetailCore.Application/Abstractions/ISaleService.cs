using RetailCore.Contracts.Sales;

namespace RetailCore.Application.Abstractions;

public interface ISaleService
{
    Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SaleDto>> GetSalesAsync(long? storeId, CancellationToken ct = default);
    Task<SaleDetailDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ReceiptDto> GetReceiptAsync(long saleId, CancellationToken ct = default);
}
