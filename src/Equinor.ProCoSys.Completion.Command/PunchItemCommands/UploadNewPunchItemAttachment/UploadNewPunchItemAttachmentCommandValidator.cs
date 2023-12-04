using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;

public class UploadNewPunchItemAttachmentCommandValidator : AbstractValidator<UploadNewPunchItemAttachmentCommand>
{
    public UploadNewPunchItemAttachmentCommandValidator(
        IPunchItemValidator punchItemValidator,
        IAttachmentService attachmentService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item with this guid does not exist! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .MustAsync((command, cancellationToken) => NotHaveAttachmentWithFileNameAsync(command.PunchItemGuid, command.FileName, cancellationToken))
            .WithMessage(command => $"Punch item already has an attachment with filename {command.FileName}! Please rename file or choose to overwrite")
            .MustAsync((command, cancellationToken) => NotBeVerifiedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item attachments can't be changed. The punch item is verified! Guid={command.PunchItemGuid}");

        async Task<bool> NotBeInAClosedProjectForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.ProjectOwningPunchItemIsClosedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.TagOwningPunchItemIsVoidedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotHaveAttachmentWithFileNameAsync(Guid punchItemGuid, string fileName, CancellationToken cancellationToken)
            => !await attachmentService.FileNameExistsForParentAsync(punchItemGuid, fileName, cancellationToken);

        async Task<bool> NotBeVerifiedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.IsVerifiedAsync(punchItemGuid, cancellationToken);
    }
}
