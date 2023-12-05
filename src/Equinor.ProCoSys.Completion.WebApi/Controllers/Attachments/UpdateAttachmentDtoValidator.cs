using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;

public class UpdateAttachmentDtoValidator : AbstractValidator<UpdateAttachmentDto>
{
    public UpdateAttachmentDtoValidator(IRowVersionInputValidator rowVersionValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Description)
            .NotNull()
            .MaximumLength(Domain.AggregateModels.AttachmentAggregate.Attachment.DescriptionLengthMax);

        RuleFor(dto => dto.Labels)
            .NotNull();

        RuleForEach(dto => dto.Labels)
            .NotNull();

        RuleFor(dto => dto.Labels)
            .Must(BeUniqueLabel)
            .WithMessage("Labels must be unique!");

        RuleFor(dto => dto.RowVersion)
            .NotNull()
            .Must(HaveValidRowVersion)
            .WithMessage(dto => $"Dto does not have valid rowVersion! RowVersion={dto.RowVersion}");

        bool HaveValidRowVersion(string rowVersion)
            => rowVersionValidator.IsValid(rowVersion);

        bool BeUniqueLabel(IList<string> labels) => labels.Distinct().Count() == labels.Count;
    }
}
