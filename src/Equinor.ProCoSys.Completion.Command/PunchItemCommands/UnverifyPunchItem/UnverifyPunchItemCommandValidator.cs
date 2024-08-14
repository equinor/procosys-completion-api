using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;

public class UnverifyPunchItemCommandValidator : AbstractValidator<UnverifyPunchItemCommand>
{
    public UnverifyPunchItemCommandValidator(ICheckListValidator checkListValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForCheckListAsync(command.PunchItem.CheckListGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item can not be unverified. The punch item is not verified! Guid={command.PunchItemGuid}");

        async Task<bool> NotBeInAVoidedTagForCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
            => !await checkListValidator.TagOwningCheckListIsVoidedAsync(checkListGuid, cancellationToken);
    }
}
