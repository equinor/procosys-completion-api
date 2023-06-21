using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunch;

public class DeletePunchCommandValidator : AbstractValidator<DeletePunchCommand>
{
    public DeletePunchCommandValidator(
        IProjectValidator projectValidator,
        IPunchValidator punchValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage(command => $"Punch with this guid does not exist! Guid={command.PunchGuid}")
            .MustAsync((command, cancellationToken) => BeAVoidedPunch(command.PunchGuid, cancellationToken))
            .WithMessage("Punch must be voided before delete!");

        async Task<bool> NotBeInAClosedProjectForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedForPunch(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.PunchExistsAsync(punchGuid, cancellationToken);

        async Task<bool> BeAVoidedPunch(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.TagOwingPunchIsVoidedAsync(punchGuid, cancellationToken);
    }
}
