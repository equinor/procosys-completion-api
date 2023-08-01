using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public class CreatePunchItemDtoValidator : AbstractValidator<CreatePunchItemDto>
{
    public CreatePunchItemDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Description)
            .NotNull()
            .MaximumLength(Domain.AggregateModels.PunchItemAggregate.PunchItem.DescriptionLengthMax);
    }
}
