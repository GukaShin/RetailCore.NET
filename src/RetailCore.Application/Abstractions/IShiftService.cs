using RetailCore.Contracts.Shifts;

namespace RetailCore.Application.Abstractions;

public interface IShiftService
{
    Task<ShiftDto> OpenAsync(OpenShiftRequest request, CancellationToken ct = default);
    Task<ShiftDto> CloseAsync(long shiftId, CloseShiftRequest request, CancellationToken ct = default);
    Task<ShiftDto?> GetCurrentAsync(CancellationToken ct = default);
    Task<ShiftDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<ShiftDto>> GetByStoreAsync(long storeId, CancellationToken ct = default);
}
