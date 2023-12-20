using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public class RejectPunchItemDtoValidator : AbstractValidator<RejectPunchItemDto>
{
    public RejectPunchItemDtoValidator(IRowVersionInputValidator rowVersionValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Comment)
            .NotNull()
            .MinimumLength(1)
            .MaximumLength(Domain.AggregateModels.CommentAggregate.Comment.TextLengthMax);

        RuleFor(dto => dto.RowVersion)
            .NotNull()
            .Must(HaveValidRowVersion)
            .WithMessage(dto => $"Dto does not have valid rowVersion! RowVersion={dto.RowVersion}");

        bool HaveValidRowVersion(string rowVersion)
            => rowVersionValidator.IsValid(rowVersion);
    }
}
