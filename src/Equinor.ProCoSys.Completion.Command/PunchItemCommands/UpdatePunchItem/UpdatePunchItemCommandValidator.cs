using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommandValidator : AbstractValidator<UpdatePunchItemCommand>
{
    public UpdatePunchItemCommandValidator(IPunchItemValidator punchItemValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        // todo 104046 Business validations for patchable properties as RaisedByOrgGuid etc. Must check if Guid exists
        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item with this guid does not exist! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!");

        async Task<bool> NotBeInAClosedProjectForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.ProjectOwningPunchItemIsClosedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.TagOwningPunchItemIsVoidedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);
    }
}
