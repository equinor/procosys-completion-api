using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;

public class ClearPunchItemCommandValidator : AbstractValidator<ClearPunchItemCommand>
{
    public ClearPunchItemCommandValidator(IPunchItemValidator punchValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item with this guid does not exist! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .MustAsync((command, cancellationToken) => IsReadyToBeClearedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item can not be cleared! Guid={command.PunchItemGuid}");

        async Task<bool> NotBeInAClosedProjectForPunchAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchValidator.ProjectOwningPunchItemIsClosedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchValidator.TagOwningPunchItemIsVoidedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchValidator.ExistsAsync(punchItemGuid, cancellationToken);

        async Task<bool> IsReadyToBeClearedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchValidator.IsReadyToBeClearedAsync(punchItemGuid, cancellationToken);
    }
}
