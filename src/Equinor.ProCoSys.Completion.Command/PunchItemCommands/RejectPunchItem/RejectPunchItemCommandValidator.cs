using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommandValidator : AbstractValidator<RejectPunchItemCommand>
{
    public RejectPunchItemCommandValidator(
        ICheckListValidator checkListValidator,
        ILabelValidator labelValidator,
        IOptionsMonitor<ApplicationOptions> options)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        var rejectLabelText = options.CurrentValue.RejectLabel;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForCheckListAsync(command.PunchItem.CheckListGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item can not be rejected. The punch item is not cleared! Guid={command.PunchItemGuid}")
            .Must(command => !command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item can not be rejected. The punch item is verified! Guid={command.PunchItemGuid}")
            .MustAsync((_, cancellationToken) => RejectLabelMustExistsAsync(cancellationToken))
            .WithMessage($"The required Label '{rejectLabelText}' is not available");

        async Task<bool> NotBeInAVoidedTagForCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
            => !await checkListValidator.TagOwningCheckListIsVoidedAsync(checkListGuid, cancellationToken);

        async Task<bool> RejectLabelMustExistsAsync(CancellationToken cancellationToken)
            => await labelValidator.ExistsAsync(rejectLabelText, cancellationToken);
    }
}
