using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public class CreatePunchItemDtoValidator : AbstractValidator<CreatePunchItemDto>
{
    public CreatePunchItemDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.ItemNo)
            .NotNull()
            .MinimumLength(Domain.AggregateModels.PunchItemAggregate.PunchItem.ItemNoLengthMin)
            .MaximumLength(Domain.AggregateModels.PunchItemAggregate.PunchItem.ItemNoLengthMax);
    }
}
