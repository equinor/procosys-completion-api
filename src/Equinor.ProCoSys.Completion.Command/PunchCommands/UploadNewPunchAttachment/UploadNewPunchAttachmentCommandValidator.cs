using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UploadNewPunchAttachment;

public class UploadNewPunchAttachmentCommandValidator : AbstractValidator<UploadNewPunchAttachmentCommand>
{
    public UploadNewPunchAttachmentCommandValidator(
        IPunchValidator punchValidator,
        IAttachmentService attachmentService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage(command => $"Punch with this guid does not exist! Guid={command.PunchGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Tag owning punch is voided!")
            .MustAsync((command, _) => NotHaveAttachmentWithFilenameAsync(command.PunchGuid, command.FileName))
            .WithMessage(command => $"Punch already has an attachment with filename {command.FileName}! Please rename file or choose to overwrite");

        async Task<bool> NotBeInAClosedProjectForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.ProjectOwningPunchIsClosedAsync(punchGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.TagOwingPunchIsVoidedAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.ExistsAsync(punchGuid, cancellationToken);

        async Task<bool> NotHaveAttachmentWithFilenameAsync(Guid punchGuid, string fileName)
            => !await attachmentService.FilenameExistsForSourceAsync(punchGuid, fileName);
    }
}
