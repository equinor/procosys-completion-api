using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.CheckListValidators;
using Equinor.ProCoSys.Completion.Command.Validators.LibraryItemValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommandValidator : AbstractValidator<CreatePunchItemCommand>
{
    public CreatePunchItemCommandValidator(
        IProjectValidator projectValidator,
        ICheckListValidator checkListValidator,
        ILibraryItemValidator libraryItemValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        // todo 105393 unit tests for checklist rules
        // must exists
        // tag not voided
        // same projectguid as given projectguid

        RuleFor(command => command)
            // validate given Project
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(command => $"Project does not exist! Guid={command.ProjectGuid}")
            .MustAsync(NotBeAClosedProjectAsync)
            .WithMessage(command => $"Project is closed! Guid={command.ProjectGuid}")

            // validate given CheckList
            .MustAsync(BeAnExistingCheckListAsync)
            .WithMessage(command => $"Check list does not exist! Guid={command.CheckListGuid}")
            .MustAsync(NotBeInAVoidedTagForCheckListAsync)
            .WithMessage(command => $"Tag owning check list is voided! Guid={command.CheckListGuid}")
            .MustAsync(CheckListMustBeInProjectAsync)
            .WithMessage(command => $"Check list is not in given project! Guid={command.CheckListGuid} Project Guid={command.ProjectGuid}")

            // validate given RaisedByOrg
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(command.RaisedByOrgGuid, cancellationToken))
            .WithMessage(command
                => $"RaisedByOrg library item does not exist! Guid={command.RaisedByOrgGuid}")
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(command.RaisedByOrgGuid, cancellationToken))
            .WithMessage(command
                => $"RaisedByOrg library item is voided! Guid={command.RaisedByOrgGuid}")
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.RaisedByOrgGuid, 
                    LibraryType.COMPLETION_ORGANIZATION,
                    cancellationToken))
            .WithMessage(command =>
                $"RaisedByOrg library item is not a {LibraryType.COMPLETION_ORGANIZATION}! " +
                $"Guid={command.RaisedByOrgGuid}")

            // validate given ClearingByOrg
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(command.ClearingByOrgGuid, cancellationToken))
            .WithMessage(command
                => $"ClearingByOrg library item does not exist! Guid={command.ClearingByOrgGuid}")
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(command.ClearingByOrgGuid, cancellationToken))
            .WithMessage(command
                => $"ClearingByOrg library item is voided! Guid={command.ClearingByOrgGuid}")
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.ClearingByOrgGuid,
                    LibraryType.COMPLETION_ORGANIZATION,
                    cancellationToken))
            .WithMessage(command =>
                $"ClearingByOrg library item is not a {LibraryType.COMPLETION_ORGANIZATION}! " +
                $"Guid={command.ClearingByOrgGuid}")

            // validate Priority, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(command.PriorityGuid!.Value, cancellationToken))
            .WithMessage(command
                => $"Priority library item does not exist! Guid={command.PriorityGuid!.Value}")
            .When(command => command.PriorityGuid.HasValue, ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(command.PriorityGuid!.Value, cancellationToken))
            .WithMessage(command
                => $"Priority library item is voided! Guid={command.PriorityGuid!.Value}")
            .When(command => command.PriorityGuid.HasValue, ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.PriorityGuid!.Value,
                    LibraryType.PUNCHLIST_PRIORITY,
                    cancellationToken))
            .WithMessage(command =>
                $"Priority library item is not a {LibraryType.PUNCHLIST_PRIORITY}! " +
                $"Guid={command.PriorityGuid!.Value}")
            .When(command => command.PriorityGuid.HasValue, ApplyConditionTo.CurrentValidator)

            // validate Sorting, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(command.SortingGuid!.Value, cancellationToken))
            .WithMessage(command
                => $"Sorting library item does not exist! Guid={command.SortingGuid!.Value}")
            .When(command => command.SortingGuid.HasValue, ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(command.SortingGuid!.Value, cancellationToken))
            .WithMessage(command
                => $"Sorting library item is voided! Guid={command.SortingGuid!.Value}")
            .When(command => command.SortingGuid.HasValue, ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.SortingGuid!.Value,
                    LibraryType.PUNCHLIST_SORTING,
                    cancellationToken))
            .WithMessage(command =>
                $"Sorting library item is not a {LibraryType.PUNCHLIST_SORTING}! " +
                $"Guid={command.SortingGuid!.Value}")
            .When(command => command.SortingGuid.HasValue, ApplyConditionTo.CurrentValidator)

            // validate Type, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(command.TypeGuid!.Value, cancellationToken))
            .WithMessage(command
                => $"Type library item does not exist! Guid={command.TypeGuid!.Value}")
            .When(command => command.TypeGuid.HasValue, ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(command.TypeGuid!.Value, cancellationToken))
            .WithMessage(command
                => $"Type library item is voided! Guid={command.TypeGuid!.Value}")
            .When(command => command.TypeGuid.HasValue, ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.TypeGuid!.Value,
                    LibraryType.PUNCHLIST_TYPE,
                    cancellationToken))
            .WithMessage(command =>
                $"Type library item is not a {LibraryType.PUNCHLIST_TYPE}! " +
                $"Guid={command.TypeGuid!.Value}")
            .When(command => command.TypeGuid.HasValue, ApplyConditionTo.CurrentValidator);

        async Task<bool> BeAnExistingProjectAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(command.ProjectGuid, cancellationToken);

        async Task<bool> NotBeAClosedProjectAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedAsync(command.ProjectGuid, cancellationToken);

        async Task<bool> BeAnExistingCheckListAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => await checkListValidator.ExistsAsync(command.CheckListGuid);

        async Task<bool> NotBeInAVoidedTagForCheckListAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => !await checkListValidator.TagOwningCheckListIsVoidedAsync(command.CheckListGuid);

        async Task<bool> CheckListMustBeInProjectAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => await checkListValidator.InProjectAsync(command.CheckListGuid, command.ProjectGuid);

        async Task<bool> BeAnExistingLibraryItemAsync(Guid guid, CancellationToken cancellationToken)
            => await libraryItemValidator.ExistsAsync(guid, cancellationToken);

        async Task<bool> BeALibraryItemOfTypeAsync(Guid guid, LibraryType type, CancellationToken cancellationToken)
            => await libraryItemValidator.HasTypeAsync(guid, type, cancellationToken);

        async Task<bool> NotBeAVoidedLibraryItemAsync(Guid guid, CancellationToken cancellationToken)
            => !await libraryItemValidator.IsVoidedAsync(guid, cancellationToken);
    }
}
