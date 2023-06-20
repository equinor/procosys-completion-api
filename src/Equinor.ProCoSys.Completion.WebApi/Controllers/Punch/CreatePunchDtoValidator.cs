using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch
{
    public class CreatePunchDtoValidator : AbstractValidator<CreatePunchDto>
    {
        public CreatePunchDtoValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(dto => dto).NotNull();

            RuleFor(dto => dto.Title)
                .NotNull()
                .MinimumLength(Domain.AggregateModels.PunchAggregate.Punch.TitleLengthMin)
                .MaximumLength(Domain.AggregateModels.PunchAggregate.Punch.TitleLengthMax);
        }
    }
}
