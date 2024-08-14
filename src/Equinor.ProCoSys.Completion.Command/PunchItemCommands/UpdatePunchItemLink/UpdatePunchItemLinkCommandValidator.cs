using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;

public class UpdatePunchItemLinkCommandValidator : AbstractValidator<UpdatePunchItemLinkCommand>
{
    public UpdatePunchItemLinkCommandValidator(ICheckListValidator checkListValidator, ILinkService linkService)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingLink(command.LinkGuid, cancellationToken))
            .WithMessage(command => $"Link with this guid does not exist! Guid={command.LinkGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForCheckListAsync(command.PunchItem.CheckListGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => !command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item links can't be updated. Punch item is cleared! Guid={command.PunchItemGuid}");

        async Task<bool> NotBeInAVoidedTagForCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
            => !await checkListValidator.TagOwningCheckListIsVoidedAsync(checkListGuid, cancellationToken);

        async Task<bool> BeAnExistingLink(Guid linkGuid, CancellationToken cancellationToken)
            => await linkService.ExistsAsync(linkGuid, cancellationToken);
    }
}
