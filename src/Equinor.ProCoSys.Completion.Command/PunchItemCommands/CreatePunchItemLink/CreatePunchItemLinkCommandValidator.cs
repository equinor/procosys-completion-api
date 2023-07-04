using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;

public class CreatePunchItemLinkCommandValidator : AbstractValidator<CreatePunchItemLinkCommand>
{
    public CreatePunchItemLinkCommandValidator(IPunchItemValidator punchItemValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item with this guid does not exist! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!");

        async Task<bool> NotBeInAClosedProjectForPunchAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.ProjectOwningPunchIsClosedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.TagOwningPunchIsVoidedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);
    }
}
