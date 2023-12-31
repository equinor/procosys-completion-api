﻿using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Links;

public class CreateLinkDtoValidator : AbstractValidator<CreateLinkDto>
{
    public CreateLinkDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Title)
            .NotNull()
            .MinimumLength(1)
            .MaximumLength(Domain.AggregateModels.LinkAggregate.Link.TitleLengthMax);

        RuleFor(dto => dto.Url)
            .NotNull()
            .MinimumLength(1)
            .MaximumLength(Domain.AggregateModels.LinkAggregate.Link.UrlLengthMax);
    }
}
