using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
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
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
            .WithMessage("Tag owning punch item is voided!")
            .MustAsync((command, cancellationToken) => BeAnExistingAttachment(command.AttachmentGuid, cancellationToken))
            .WithMessage(command => $"Attachment with this guid does not exist! Guid={command.AttachmentGuid}")
            .Must(command => !command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item attachments can't be changed. The punch item is verified! Guid={command.PunchItemGuid}")
            .Unless(command => CurrentUserIsVerifier(command.PunchItem), ApplyConditionTo.CurrentValidator);

        RuleForEach(command => command.Labels)
            .MustAsync((_, label, _, token) => BeAnExistingLabelAsync(label, token))
            .WithMessage((_, label) => $"Label doesn't exist! Label={label}")
            .MustAsync((_, label, _, token) => NotBeAVoidedLabelAsync(label, token))
            .WithMessage((_, label) => $"Label is voided! Label={label}");

        async Task<bool> BeAnExistingAttachment(Guid attachmentGuid, CancellationToken cancellationToken)
            => await attachmentService.ExistsAsync(attachmentGuid, cancellationToken);

        async Task<bool> BeAnExistingLabelAsync(string label, CancellationToken cancellationToken)
            => await labelValidator.ExistsAsync(label, cancellationToken);

        async Task<bool> NotBeAVoidedLabelAsync(string label, CancellationToken cancellationToken)
            => !await labelValidator.IsVoidedAsync(label, cancellationToken);

        bool CurrentUserIsVerifier(PunchItem punchItem) => punchItemValidator.CurrentUserIsVerifier(punchItem);
    }
}
