﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;

public class DeletePunchItemAttachmentCommandValidator : AbstractValidator<DeletePunchItemAttachmentCommand>
{
    public DeletePunchItemAttachmentCommandValidator(
        IPunchItemValidator punchItemValidator,
        IAttachmentService attachmentService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item with this guid does not exist! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => BeAnExistingAttachment(command.AttachmentGuid, cancellationToken))
            .WithMessage(command => $"Attachment with this guid does not exist! Guid={command.AttachmentGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .MustAsync((command, cancellationToken) => NotBeVerifiedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item attachments can't be changed. The punch item is verified! Guid={command.PunchItemGuid}")
            .UnlessAsync((command, cancellationToken)
                => CurrentUserIsVerifier(command.PunchItemGuid, cancellationToken), ApplyConditionTo.CurrentValidator);

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

        async Task<bool> CurrentUserIsVerifier(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.CurrentUserIsVerifierAsync(punchItemGuid, cancellationToken);
    }
}
