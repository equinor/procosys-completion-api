using System.Threading.Tasks;
using System.Threading;
using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;

public class UpdatePunchItemCategoryCommandValidator : AbstractValidator<UpdatePunchItemCategoryCommand>
{
    public UpdatePunchItemCategoryCommandValidator(IPunchItemValidator punchItemValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeInAClosedProjectForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Project is closed!")
            .MustAsync((command, cancellationToken) => BeAnExistingPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item with this guid does not exist! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotBeInAVoidedTagForPunchItemAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage("Tag owning punch item is voided!")
            .MustAsync((command, cancellationToken) => NotHaveSameCategoryAsync(command.PunchItemGuid, command.Category, cancellationToken))
            .WithMessage(command => $"Punch item already have category {command.Category}! Guid={command.PunchItemGuid}")
            .MustAsync((command, cancellationToken) => NotBeClearedAsync(command.PunchItemGuid, cancellationToken))
            .WithMessage(command => $"Punch item is cleared! Guid={command.PunchItemGuid}");

        async Task<bool> NotBeInAClosedProjectForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.ProjectOwningPunchItemIsClosedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.TagOwningPunchItemIsVoidedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotHaveSameCategoryAsync(Guid punchItemGuid, Category category, CancellationToken cancellationToken)
            => !await punchItemValidator.HasCategoryAsync(punchItemGuid, category, cancellationToken);

        async Task<bool> NotBeClearedAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.IsClearedAsync(punchItemGuid, cancellationToken);
    }
}
