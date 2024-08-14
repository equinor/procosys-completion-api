using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;

public class DeletePunchItemAttachmentCommandValidator : AbstractValidator<DeletePunchItemAttachmentCommand>
{
    public DeletePunchItemAttachmentCommandValidator(
        IPunchItemValidator punchItemValidator,
        ICheckListValidator checkListValidator,
        IAttachmentService attachmentService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingAttachment(command.AttachmentGuid, cancellationToken))
            .WithMessage(command => $"Attachment with this guid does not exist! Guid={command.AttachmentGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForCheckListAsync(command.PunchItem.CheckListGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => !command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item attachments can't be deleted. The punch item is verified! Guid={command.PunchItemGuid}")
            .Unless(command => CurrentUserIsVerifier(command.PunchItem), ApplyConditionTo.CurrentValidator);

        async Task<bool> NotBeInAVoidedTagForCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
            => !await checkListValidator.TagOwningCheckListIsVoidedAsync(checkListGuid, cancellationToken);

        async Task<bool> BeAnExistingAttachment(Guid attachmentGuid, CancellationToken cancellationToken)
            => await attachmentService.ExistsAsync(attachmentGuid, cancellationToken);

        bool CurrentUserIsVerifier(PunchItem punchItem) => punchItemValidator.CurrentUserIsVerifier(punchItem);
    }
}
