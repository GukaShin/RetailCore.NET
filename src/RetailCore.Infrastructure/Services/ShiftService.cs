using Microsoft.EntityFrameworkCore;
using RetailCore.Application.Abstractions;
using RetailCore.Application.Common.Exceptions;
using RetailCore.Application.Services;
using RetailCore.Contracts.Shifts;
using RetailCore.Domain.Entities;
using RetailCore.Domain.Enums;

namespace RetailCore.Infrastructure.Services;

public class ShiftService : IShiftService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public ShiftService(IApplicationDbContext db, ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<ShiftDto> OpenAsync(OpenShiftRequest request, CancellationToken ct = default)
    {
        var cashierId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");
        var storeId = _currentUser.StoreId ?? throw new BusinessRuleException("Cashier is not assigned to a store.");

        if (await _db.CashierShifts.AnyAsync(s => s.CashierId == cashierId && s.Status == ShiftStatus.Open, ct))
        {
            throw new ConflictException("Cashier already has an open shift.");
        }

        var register = await _db.CashRegisters.FirstOrDefaultAsync(r => r.Id == request.CashRegisterId, ct)
            ?? throw new NotFoundException("CashRegister", request.CashRegisterId);

        if (register.StoreId != storeId)
        {
            throw new BusinessRuleException("Cash register does not belong to the cashier's store.");
        }

        if (register.Status != RegisterStatus.Active)
        {
            throw new BusinessRuleException("Cash register is not active.");
        }

        if (request.OpeningCashAmount < 0)
        {
            throw new BusinessRuleException("Opening cash amount cannot be negative.");
        }

        var shift = new CashierShift
        {
            CashierId = cashierId,
            StoreId = storeId,
            CashRegisterId = request.CashRegisterId,
            OpeningCashAmount = request.OpeningCashAmount,
            Status = ShiftStatus.Open,
            OpenedAt = _clock.UtcNow
        };

        _db.CashierShifts.Add(shift);
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = cashierId,
            StoreId = storeId,
            Action = "ShiftOpened",
            Description = $"Shift opened on register {register.Code} with opening cash {request.OpeningCashAmount:F2}",
            EntityName = nameof(CashierShift),
            EntityId = null,
            CreatedAt = _clock.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Map(shift);
    }

    public async Task<ShiftDto> CloseAsync(long shiftId, CloseShiftRequest request, CancellationToken ct = default)
    {
        var cashierId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");

        var shift = await _db.CashierShifts.FirstOrDefaultAsync(s => s.Id == shiftId, ct)
            ?? throw new NotFoundException("Shift", shiftId);

        if (shift.CashierId != cashierId && _currentUser.Role is not UserRole.Admin and not UserRole.StoreManager)
        {
            throw new UnauthorizedException("You can only close your own shift.");
        }

        if (shift.Status != ShiftStatus.Open)
        {
            throw new BusinessRuleException("Shift is not open.");
        }

        var expectedCash = await CalculateExpectedCashAsync(shift, ct);
        var difference = ShiftCashCalculator.CalculateDifference(request.ActualCashAmount, expectedCash);

        shift.ExpectedCashAmount = expectedCash;
        shift.ActualCashAmount = request.ActualCashAmount;
        shift.DifferenceAmount = difference;
        shift.Status = ShiftStatus.Closed;
        shift.ClosedAt = _clock.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = cashierId,
            StoreId = shift.StoreId,
            Action = "ShiftClosed",
            Description = $"Shift closed. Expected {expectedCash:F2}, actual {request.ActualCashAmount:F2}, difference {difference:F2}",
            EntityName = nameof(CashierShift),
            EntityId = shift.Id.ToString(),
            CreatedAt = _clock.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Map(shift);
    }

    public async Task<ShiftDto?> GetCurrentAsync(CancellationToken ct = default)
    {
        var cashierId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");

        var shift = await _db.CashierShifts
            .FirstOrDefaultAsync(s => s.CashierId == cashierId && s.Status == ShiftStatus.Open, ct);

        return shift is null ? null : Map(shift);
    }

    public async Task<ShiftDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var shift = await _db.CashierShifts.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Shift", id);
        return Map(shift);
    }

    public async Task<IReadOnlyList<ShiftDto>> GetByStoreAsync(long storeId, CancellationToken ct = default)
    {
        if (!await _db.Stores.AnyAsync(s => s.Id == storeId, ct))
        {
            throw new NotFoundException("Store", storeId);
        }

        var shifts = await _db.CashierShifts
            .Where(s => s.StoreId == storeId)
            .OrderByDescending(s => s.OpenedAt)
            .ToListAsync(ct);

        return shifts.Select(Map).ToList();
    }

    private async Task<decimal> CalculateExpectedCashAsync(CashierShift shift, CancellationToken ct)
    {
        var cashSales = await _db.Payments
            .Where(p => p.PaymentMethod == PaymentMethod.Cash
                        && p.Status == PaymentStatus.Succeeded
                        && p.Sale!.ShiftId == shift.Id
                        && p.Sale.SaleStatus == SaleStatus.Completed)
            .SumAsync(p => p.Amount, ct);

        // Refund cash impact is added in a later phase.
        return ShiftCashCalculator.CalculateExpectedCash(shift.OpeningCashAmount, cashSales, 0);
    }

    private static ShiftDto Map(CashierShift s) => new(
        s.Id,
        s.CashierId,
        s.StoreId,
        s.CashRegisterId,
        s.OpeningCashAmount,
        s.ExpectedCashAmount,
        s.ActualCashAmount,
        s.DifferenceAmount,
        s.Status.ToString(),
        s.OpenedAt,
        s.ClosedAt);
}
