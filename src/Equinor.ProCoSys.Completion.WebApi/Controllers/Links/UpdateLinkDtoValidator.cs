using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Links;

public class UpdateLinkDtoValidator : AbstractValidator<UpdateLinkDto>
{
    public UpdateLinkDtoValidator(IRowVersionInputValidator rowVersionValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Title)
            .NotNull()
            .MaximumLength(Domain.AggregateModels.LinkAggregate.Link.TitleLengthMax);

        RuleFor(dto => dto.Url)
            .NotNull()
            .MaximumLength(Domain.AggregateModels.LinkAggregate.Link.UrlLengthMax);

        RuleFor(dto => dto.RowVersion)
            .NotNull()
            .Must(HaveValidRowVersion)
            .WithMessage(dto => $"Dto does not have valid rowVersion! RowVersion={dto.RowVersion}");

        bool HaveValidRowVersion(string rowVersion)
            => rowVersionValidator.IsValid(rowVersion);
    }
}
