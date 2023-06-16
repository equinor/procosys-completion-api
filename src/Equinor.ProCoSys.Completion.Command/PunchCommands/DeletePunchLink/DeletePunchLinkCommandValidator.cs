using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchLink;

public class DeletePunchLinkCommandValidator : AbstractValidator<DeletePunchLinkCommand>
{
    public DeletePunchLinkCommandValidator(
        IProjectValidator projectValidator,
        IPunchValidator punchValidator,
        ILinkService linkService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeAClosedProjectForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunch(command.PunchGuid, cancellationToken))
            .WithMessage(command => $"Punch with this guid does not exist! Guid={command.PunchGuid}")
            .MustAsync((command, _) => BeAnExistingLink(command.LinkGuid))
            .WithMessage(command => $"Link with this guid does not exist! Guid={command.LinkGuid}")
            .MustAsync((command, cancellationToken) => NotBeAVoidedPunch(command.PunchGuid, cancellationToken))
            .WithMessage("Punch is voided!");

        async Task<bool> NotBeAClosedProjectForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedForPunch(punchGuid, cancellationToken);

        async Task<bool> NotBeAVoidedPunch(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.PunchIsVoidedAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingPunch(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.PunchExistsAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingLink(Guid linkGuid)
            => await linkService.ExistsAsync(linkGuid);
    }
}
