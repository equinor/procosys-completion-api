using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Query.Attachments;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;

public class GetPunchItemAttachmentDownloadUrlQueryValidator : AbstractValidator<GetPunchItemAttachmentDownloadUrlQuery>
{
    public GetPunchItemAttachmentDownloadUrlQueryValidator(
        IPunchItemValidator punchItemValidator,
        IAttachmentService attachmentService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query)
            .MustAsync((query, cancellationToken) => BeAnExistingPunchItemAsync(query.PunchItemGuid, cancellationToken))
            .WithMessage(query => $"Punch item with this guid does not exist! Guid={query.PunchItemGuid}")
            .WithState(_ => new EntityNotFoundException())
            .MustAsync((query, cancellationToken) => BeAnExistingAttachment(query.AttachmentGuid, cancellationToken))
            .WithMessage(query => $"Attachment with this guid does not exist! Guid={query.AttachmentGuid}")
            .WithState(_ => new EntityNotFoundException());

        async Task<bool> BeAnExistingPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingAttachment(Guid attachmentGuid, CancellationToken cancellationToken)
            => await attachmentService.ExistsAsync(attachmentGuid, cancellationToken);
    }
}
