using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;

public class VerifyPunchItemCommandValidator : AbstractValidator<VerifyPunchItemCommand>
{
    public VerifyPunchItemCommandValidator(IPunchItemValidator punchItemValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item with this guid does not exist! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .MustAsync((command, cancellationToken) => BeClearedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item can not be verified. The punch item is not cleared! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotAlreadyBeVerifiedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item is already verified! Guid={command.PunchItemGuid}");

        async Task<bool> NotBeInAClosedProjectForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.ProjectOwningPunchItemIsClosedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.TagOwningPunchItemIsVoidedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeClearedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.IsClearedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotAlreadyBeVerifiedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.IsVerifiedAsync(punchItemGuid, cancellationToken);
    }

}
