using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;

public class UploadNewPunchItemAttachmentCommandValidator : AbstractValidator<UploadNewPunchItemAttachmentCommand>
{
    public UploadNewPunchItemAttachmentCommandValidator(
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
            .MustAsync((command, cancellationToken) => NotHaveAttachmentWithFileNameAsync(command.PunchItemGuid, command.FileName, cancellationToken))
            .WithMessage(command => $"Punch item already has an attachment with filename {command.FileName}! Please rename file or choose to overwrite")
            .Must(command => !command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item attachments can't be changed. The punch item is verified! Guid={command.PunchItemGuid}")
            .Unless(command => CurrentUserIsVerifier(command.PunchItem), ApplyConditionTo.CurrentValidator);

        async Task<bool> NotBeInAVoidedTagForCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
            => !await checkListValidator.TagOwningCheckListIsVoidedAsync(checkListGuid, cancellationToken);

        async Task<bool> NotHaveAttachmentWithFileNameAsync(Guid punchItemGuid, string fileName, CancellationToken cancellationToken)
            => !await attachmentService.FileNameExistsForParentAsync(punchItemGuid, fileName, cancellationToken);

        bool CurrentUserIsVerifier(PunchItem punchItem) => punchItemValidator.CurrentUserIsVerifier(punchItem);
    }
}
