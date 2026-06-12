using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailCore.Api.Authorization;
using RetailCore.Application.Abstractions;
using RetailCore.Contracts.Shifts;

namespace RetailCore.Api.Controllers;

[ApiController]
[Route("api/shifts")]
[Authorize(Policy = RolePolicies.CashierOperations)]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shifts;

    public ShiftsController(IShiftService shifts) => _shifts = shifts;

    [HttpPost("open")]
    public async Task<ActionResult<ShiftDto>> Open(OpenShiftRequest request, CancellationToken ct)
        => Ok(await _shifts.OpenAsync(request, ct));

    [HttpPost("{id:long}/close")]
    public async Task<ActionResult<ShiftDto>> Close(long id, CloseShiftRequest request, CancellationToken ct)
        => Ok(await _shifts.CloseAsync(id, request, ct));

    [HttpGet("my-current")]
    public async Task<ActionResult<ShiftDto?>> GetCurrent(CancellationToken ct)
        => Ok(await _shifts.GetCurrentAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ShiftDto>> GetById(long id, CancellationToken ct)
        => Ok(await _shifts.GetByIdAsync(id, ct));

    [HttpGet("/api/stores/{storeId:long}/shifts")]
    [Authorize(Policy = RolePolicies.Management)]
    public async Task<ActionResult<IReadOnlyList<ShiftDto>>> GetByStore(long storeId, CancellationToken ct)
        => Ok(await _shifts.GetByStoreAsync(storeId, ct));
}
