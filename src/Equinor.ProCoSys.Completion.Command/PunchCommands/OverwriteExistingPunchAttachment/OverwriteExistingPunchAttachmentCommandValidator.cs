using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.OverwriteExistingPunchAttachment;

public class OverwriteExistingPunchAttachmentCommandValidator : AbstractValidator<OverwriteExistingPunchAttachmentCommand>
{
    public OverwriteExistingPunchAttachmentCommandValidator(
        IProjectValidator projectValidator,
        IPunchValidator punchValidator,
        IAttachmentService attachmentService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeAClosedProjectForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunch(command.PunchGuid, cancellationToken))
            .WithMessage(command => $"Punch with this guid does not exist! Guid={command.PunchGuid}")
            .MustAsync((command, cancellationToken) => NotBeAVoidedPunch(command.PunchGuid, cancellationToken))
            .WithMessage("Punch is voided!")
            .MustAsync((command, _) => HaveAttachmentWithFilenameAsync(command.PunchGuid, command.FileName))
            .WithMessage(command => $"Punch don't have an attachment with filename {command.FileName}!");

        async Task<bool> NotBeAClosedProjectForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedForPunch(punchGuid, cancellationToken);

        async Task<bool> NotBeAVoidedPunch(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.PunchIsVoidedAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingPunch(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.PunchExistsAsync(punchGuid, cancellationToken);

        async Task<bool> HaveAttachmentWithFilenameAsync(Guid punchGuid, string fileName)
            => await attachmentService.FilenameExistsForSourceAsync(punchGuid, fileName);
    }
}
