using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;

public class UpdateLabelAvailableForDtoValidator : AbstractValidator<UpdateLabelAvailableForDto>
{
    public UpdateLabelAvailableForDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Text)
            .NotNull();

        RuleFor(dto => dto.AvailableForLabels)
            .NotNull();
    }
}
