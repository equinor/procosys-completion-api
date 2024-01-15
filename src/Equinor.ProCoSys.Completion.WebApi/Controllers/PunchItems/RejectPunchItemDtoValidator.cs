using System.Collections.Generic;
using System;
using System.Linq;
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

        RuleFor(dto => dto.Mentions)
            .NotNull();

        RuleFor(dto => dto.Mentions)
            .Must(BeUniqueMentions)
            .WithMessage("Mentions must be unique!");

        RuleFor(dto => dto.RowVersion)
            .NotNull()
            .Must(HaveValidRowVersion)
            .WithMessage(dto => $"Dto does not have valid rowVersion! RowVersion={dto.RowVersion}");

        bool BeUniqueMentions(IList<Guid> mentions) => mentions.Distinct().Count() == mentions.Count;
        bool HaveValidRowVersion(string rowVersion) => rowVersionValidator.IsValid(rowVersion);
    }
}
