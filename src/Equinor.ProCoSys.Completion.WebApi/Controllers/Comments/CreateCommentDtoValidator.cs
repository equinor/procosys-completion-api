﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Comments;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Text)
            .NotNull()
            .MinimumLength(1)
            .MaximumLength(Domain.AggregateModels.CommentAggregate.Comment.TextLengthMax);

        RuleFor(dto => dto.Labels)
            .NotNull();

        RuleForEach(dto => dto.Labels)
            .NotNull();

        RuleFor(dto => dto.Labels)
            .Must(BeUniqueLabels)
            .WithMessage("Labels must be unique!");

        RuleFor(dto => dto.Mentions)
            .NotNull();

        RuleFor(dto => dto.Mentions)
            .Must(BeUniqueMentions)
            .WithMessage("Mentions must be unique!");

        bool BeUniqueLabels(IList<string> labels) => labels.Distinct().Count() == labels.Count;
        bool BeUniqueMentions(IList<Guid> mentions) => mentions.Distinct().Count() == mentions.Count;
    }
}
