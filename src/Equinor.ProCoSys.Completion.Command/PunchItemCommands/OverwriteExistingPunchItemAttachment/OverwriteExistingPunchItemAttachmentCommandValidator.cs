using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;

public class OverwriteExistingPunchItemAttachmentCommandValidator : AbstractValidator<OverwriteExistingPunchItemAttachmentCommand>
{
    public OverwriteExistingPunchItemAttachmentCommandValidator(
        IPunchItemValidator punchItemValidator, 
        ICheckListValidator checkListValidator,
        IAttachmentService attachmentService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForCheckListAsync(command.PunchItem.CheckListGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .MustAsync((command, cancellationToken) => HaveAttachmentWithFileNameAsync(command.PunchItemGuid, command.FileName, cancellationToken))
            .WithMessage(command => $"Punch item don't have an attachment with filename {command.FileName}!")
            .Must(command => !command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item attachments can't be changed. The punch item is verified! Guid={command.PunchItemGuid}")
            .Unless(command => CurrentUserIsVerifier(command.PunchItem), ApplyConditionTo.CurrentValidator);

        async Task<bool> NotBeInAVoidedTagForCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
            => !await checkListValidator.TagOwningCheckListIsVoidedAsync(checkListGuid, cancellationToken);

        async Task<bool> HaveAttachmentWithFileNameAsync(Guid punchItemGuid, string fileName, CancellationToken cancellationToken)
            => await attachmentService.FileNameExistsForParentAsync(punchItemGuid, fileName, cancellationToken);

        bool CurrentUserIsVerifier(PunchItem punchItem) => punchItemValidator.CurrentUserIsVerifier(punchItem);
    }
}
