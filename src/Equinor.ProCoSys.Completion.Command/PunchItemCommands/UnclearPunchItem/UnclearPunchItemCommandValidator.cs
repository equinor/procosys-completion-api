using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;

public class UnclearPunchItemCommandValidator : AbstractValidator<UnclearPunchItemCommand>
{
    public UnclearPunchItemCommandValidator(ICheckListValidator checkListValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForCheckListAsync(command.PunchItem.CheckListGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item can not be uncleared. The punch item is not cleared! Guid={command.PunchItemGuid}")
            .Must(command => !command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item can not be uncleared. The punch item is verified! Guid={command.PunchItemGuid}");

        async Task<bool> NotBeInAVoidedTagForCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
            => !await checkListValidator.TagOwningCheckListIsVoidedAsync(checkListGuid, cancellationToken);
    }
}
