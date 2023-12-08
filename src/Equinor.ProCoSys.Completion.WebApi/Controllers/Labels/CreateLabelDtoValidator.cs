using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;

public class CreateLabelDtoValidator : AbstractValidator<CreateLabelDto>
{
    public CreateLabelDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Text)
            .NotNull()
            .MaximumLength(Domain.AggregateModels.LabelAggregate.Label.TextLengthMax);
    }
}
