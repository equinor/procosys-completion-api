using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunchLink;

public class UpdatePunchLinkCommandValidator : AbstractValidator<UpdatePunchLinkCommand>
{
    public UpdatePunchLinkCommandValidator(
        IPunchValidator punchValidator,
        ILinkService linkService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage(command => $"Punch with this guid does not exist! Guid={command.PunchGuid}")
            .MustAsync((command, _) => BeAnExistingLink(command.LinkGuid))
            .WithMessage(command => $"Link with this guid does not exist! Guid={command.LinkGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchAsync(command.PunchGuid, cancellationToken))
            .WithMessage("Tag owning punch is voided!");

        async Task<bool> NotBeInAClosedProjectForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.ProjectOwningPunchIsClosedAsync(punchGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => !await punchValidator.TagOwningPunchIsVoidedAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchAsync(Guid punchGuid, CancellationToken cancellationToken)
            => await punchValidator.ExistsAsync(punchGuid, cancellationToken);

        async Task<bool> BeAnExistingLink(Guid linkGuid)
            => await linkService.ExistsAsync(linkGuid);
    }
}
