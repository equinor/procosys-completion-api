using System;
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

        RuleFor(dto => dto.DueTimeUtc)
            .Must(dt => dt!.Value.Kind == DateTimeKind.Utc)
            .When(dto => dto.DueTimeUtc.HasValue, ApplyConditionTo.CurrentValidator)
            .WithMessage(dto => $"{nameof(dto.DueTimeUtc)} must be UTC");

        RuleFor(dto => dto.MaterialETAUtc)
            .Must(dt => dt!.Value.Kind == DateTimeKind.Utc)
            .When(dto => dto.MaterialETAUtc.HasValue, ApplyConditionTo.CurrentValidator)
            .WithMessage(dto => $"{nameof(dto.MaterialETAUtc)} must be UTC");
    }
}
