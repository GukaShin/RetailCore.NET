using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCore.Api.Authorization;
using RetailCore.Application.Abstractions;
using RetailCore.Contracts.Sales;

namespace RetailCore.Api.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _sales;

    public SalesController(ISaleService sales) => _sales = sales;

    [HttpPost("checkout")]
    [Authorize(Policy = RolePolicies.CashierOperations)]
    public async Task<ActionResult<CheckoutResponse>> Checkout(CheckoutRequest request, CancellationToken ct)
        => Ok(await _sales.CheckoutAsync(request, ct));

    [HttpGet]
    [Authorize(Policy = RolePolicies.Management)]
    public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetSales([FromQuery] long? storeId, CancellationToken ct)
        => Ok(await _sales.GetSalesAsync(storeId, ct));

    [HttpGet("{id:long}")]
    [Authorize(Policy = RolePolicies.CashierOperations)]
    public async Task<ActionResult<SaleDetailDto>> GetById(long id, CancellationToken ct)
        => Ok(await _sales.GetByIdAsync(id, ct));

    [HttpGet("{id:long}/receipt")]
    [Authorize(Policy = RolePolicies.CashierOperations)]
    public async Task<ActionResult<ReceiptDto>> GetReceipt(long id, CancellationToken ct)
        => Ok(await _sales.GetReceiptAsync(id, ct));
}
