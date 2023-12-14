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
        IPunchItemValidator punchItemValidator,
        ILabelValidator labelValidator,
        IOptionsMonitor<ApplicationOptions> options)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        var rejectLabelText = options.CurrentValue.RejectLabel;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item with this guid does not exist! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .MustAsync((command, cancellationToken) => BeClearedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item can not be rejected. The punch item is not cleared! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotAlreadyBeVerifiedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item can not be rejected. The punch item is verified! Guid={command.PunchItemGuid}")
            .MustAsync((_, cancellationToken) => RejectLabelMustExistsAsync(cancellationToken))
            .WithMessage($"The required Label '{rejectLabelText}' is not available");

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

        async Task<bool> RejectLabelMustExistsAsync(CancellationToken cancellationToken)
            => await labelValidator.ExistsAsync(rejectLabelText, cancellationToken);
    }
}
