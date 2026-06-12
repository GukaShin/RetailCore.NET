using FluentValidation;
using RetailCore.Contracts.Shifts;

namespace RetailCore.Application.Features.Shifts;

public class OpenShiftRequestValidator : AbstractValidator<OpenShiftRequest>
{
    public OpenShiftRequestValidator()
    {
        RuleFor(x => x.CashRegisterId).GreaterThan(0);
        RuleFor(x => x.OpeningCashAmount).GreaterThanOrEqualTo(0);
    }
}

public class CloseShiftRequestValidator : AbstractValidator<CloseShiftRequest>
{
    public CloseShiftRequestValidator()
    {
        RuleFor(x => x.ActualCashAmount).GreaterThanOrEqualTo(0);
    }
}
