using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchLink;

public class CreatePunchLinkCommandValidator : AbstractValidator<CreatePunchLinkCommand>
{
    public CreatePunchLinkCommandValidator(
        IProjectValidator projectValidator,
        IPunchValidator punchValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeAClosedProjectForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunch(command.PunchGuid, cancellationToken))
            .WithMessage(command => $"Punch with this guid does not exist! Guid={command.PunchGuid}")
            .MustAsync((command, cancellationToken) => NotBeAVoidedPunch(command.PunchGuid, cancellationToken))
            .WithMessage("Punch is voided!");

        async Task<bool> NotBeAClosedProjectForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedForPunch(punchGuid, cancellationToken);

        async Task<bool> NotBeAVoidedPunch(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.PunchIsVoidedAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingPunch(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.PunchExistsAsync(punchGuid, cancellationToken);
    }
}
