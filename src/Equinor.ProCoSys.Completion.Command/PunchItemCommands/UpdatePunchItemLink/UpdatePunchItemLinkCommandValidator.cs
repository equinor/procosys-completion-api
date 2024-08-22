using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;

public class UpdatePunchItemLinkCommandValidator : AbstractValidator<UpdatePunchItemLinkCommand>
{
    public UpdatePunchItemLinkCommandValidator(ILinkService linkService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingLink(command.LinkGuid, cancellationToken))
            .WithMessage(command => $"Link with this guid does not exist! Guid={command.LinkGuid}")
            .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => !command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item links can't be updated. Punch item is cleared! Guid={command.PunchItemGuid}");

        async Task<bool> BeAnExistingLink(Guid linkGuid, CancellationToken cancellationToken)
            => await linkService.ExistsAsync(linkGuid, cancellationToken);
    }
}
