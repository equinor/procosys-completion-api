using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;

public class UpdatePunchItemAttachmentCommandValidator : AbstractValidator<UpdatePunchItemAttachmentCommand>
{
    public UpdatePunchItemAttachmentCommandValidator(
        IPunchItemValidator punchItemValidator,
        ILabelValidator labelValidator,
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
            .MustAsync((command, cancellationToken) => BeAnExistingAttachment(command.AttachmentGuid, cancellationToken))
            .WithMessage(command => $"Attachment with this guid does not exist! Guid={command.AttachmentGuid}")
            .MustAsync((command, cancellationToken) => NotBeVerifiedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item attachments can't be changed. The punch item is verified! Guid={command.PunchItemGuid}")
            .UnlessAsync((command, cancellationToken)
                => CurrentUserIsVerifier(command.PunchItemGuid, cancellationToken), ApplyConditionTo.CurrentValidator);

        RuleForEach(command => command.Labels)
            .MustAsync((_, label, _, token) => BeAnExistingLabelAsync(label, token))
            .WithMessage((_, label) => $"Label doesn't exist! Label={label}")
            .MustAsync((_, label, _, token) => NotBeAVoidedLabelAsync(label, token))
            .WithMessage((_, label) => $"Label is voided! Label={label}");

        async Task<bool> NotBeInAClosedProjectForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.ProjectOwningPunchItemIsClosedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.TagOwningPunchItemIsVoidedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingAttachment(Guid attachmentGuid, CancellationToken cancellationToken)
            => await attachmentService.ExistsAsync(attachmentGuid, cancellationToken);

        async Task<bool> NotBeVerifiedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.IsVerifiedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingLabelAsync(string label, CancellationToken cancellationToken)
            => await labelValidator.ExistsAsync(label, cancellationToken);

        async Task<bool> NotBeAVoidedLabelAsync(string label, CancellationToken cancellationToken)
            => !await labelValidator.IsVoidedAsync(label, cancellationToken);

        async Task<bool> CurrentUserIsVerifier(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.CurrentUserIsVerifierAsync(punchItemGuid, cancellationToken);
    }
}
