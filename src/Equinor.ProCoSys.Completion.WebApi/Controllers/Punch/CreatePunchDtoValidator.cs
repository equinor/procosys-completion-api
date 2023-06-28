using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;

public class CreatePunchDtoValidator : AbstractValidator<CreatePunchDto>
{
    public CreatePunchDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.ItemNo)
            .NotNull()
            .MinimumLength(Domain.AggregateModels.PunchAggregate.Punch.ItemNoLengthMin)
            .MaximumLength(Domain.AggregateModels.PunchAggregate.Punch.ItemNoLengthMax);
    }
}