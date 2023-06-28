using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchAttachment;

public class DeletePunchAttachmentCommandValidator : AbstractValidator<DeletePunchAttachmentCommand>
{
    public DeletePunchAttachmentCommandValidator(
        IPunchValidator punchValidator,
        IAttachmentService attachmentService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage(command => $"Punch with this guid does not exist! Guid={command.PunchGuid}")
            .MustAsync((command, _) => BeAnExistingAttachment(command.AttachmentGuid))
            .WithMessage(command => $"Attachment with this guid does not exist! Guid={command.AttachmentGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Tag owning punch is voided!");

        async Task<bool> NotBeInAClosedProjectForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.ProjectOwningPunchIsClosedAsync(punchGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.TagOwningPunchIsVoidedAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.ExistsAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingAttachment(Guid attachmentGuid)
            => await attachmentService.ExistsAsync(attachmentGuid);
    }
}
