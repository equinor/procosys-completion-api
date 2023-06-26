using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunch;

public class DeletePunchCommandValidator : AbstractValidator<DeletePunchCommand>
{
    public DeletePunchCommandValidator(IPunchValidator punchValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage(command => $"Punch with this guid does not exist! Guid={command.PunchGuid}");

        async Task<bool> NotBeInAClosedProjectForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.ProjectOwningPunchIsClosedAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.ExistsAsync(punchGuid, cancellationToken);
    }
}
