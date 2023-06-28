using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;

public class UpdatePunchDtoValidator : AbstractValidator<UpdatePunchDto>
{
    public UpdatePunchDtoValidator(IRowVersionValidator rowVersionValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Description)
            .MaximumLength(Domain.AggregateModels.PunchAggregate.Punch.DescriptionLengthMax);

        RuleFor(dto => dto.RowVersion)
            .NotNull()
            .Must(HaveValidRowVersion)
            .WithMessage(dto => $"Dto does not have valid rowVersion! RowVersion={dto.RowVersion}");

        bool HaveValidRowVersion(string rowVersion)
            => rowVersionValidator.IsValid(rowVersion);
    }
}
