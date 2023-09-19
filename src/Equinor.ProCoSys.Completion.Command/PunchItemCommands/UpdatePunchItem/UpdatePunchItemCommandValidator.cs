using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommandValidator : AbstractValidator<UpdatePunchItemCommand>
{
    // Business Validation is based on that Input Validation is done in advance, thus all replaced ..
    // ... guid values are validated to be Guids
    public UpdatePunchItemCommandValidator(IPunchItemValidator punchItemValidator, ILibraryItemValidator libraryItemValidator)
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

            // validate RaisedByOrg, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.RaisedByOrgGuid),
                    cancellationToken))
            .WithMessage(command
                => $"RaisedByOrg library item does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.RaisedByOrgGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                command.PatchDocument.Operations,
                nameof(PatchablePunchItem.RaisedByOrgGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.RaisedByOrgGuid),
                    cancellationToken))
            .WithMessage(command
                => $"RaisedByOrg library item is voided! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.RaisedByOrgGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.RaisedByOrgGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.RaisedByOrgGuid),
                    LibraryType.COMPLETION_ORGANIZATION,
                    cancellationToken))
            .WithMessage(command =>
                $"RaisedByOrg library item is not a {LibraryType.COMPLETION_ORGANIZATION}! " +
                $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.RaisedByOrgGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.RaisedByOrgGuid)),
                ApplyConditionTo.CurrentValidator)

            // validate ClearingByOrg, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.ClearingByOrgGuid),
                    cancellationToken))
            .WithMessage(command
                => $"ClearingByOrg library item does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.ClearingByOrgGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                command.PatchDocument.Operations,
                nameof(PatchablePunchItem.ClearingByOrgGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.ClearingByOrgGuid),
                    cancellationToken))
            .WithMessage(command
                => $"ClearingByOrg library item is voided! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.ClearingByOrgGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.ClearingByOrgGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.ClearingByOrgGuid),
                    LibraryType.COMPLETION_ORGANIZATION,
                    cancellationToken))
            .WithMessage(command =>
                $"ClearingByOrg library item is not a {LibraryType.COMPLETION_ORGANIZATION}! " +
                $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.ClearingByOrgGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.ClearingByOrgGuid)),
                ApplyConditionTo.CurrentValidator)

            // validate Priority, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.PriorityGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Priority library item does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.PriorityGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                command.PatchDocument.Operations,
                nameof(PatchablePunchItem.PriorityGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.PriorityGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Priority library item is voided! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.PriorityGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.PriorityGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.PriorityGuid),
                    LibraryType.PUNCHLIST_PRIORITY,
                    cancellationToken))
            .WithMessage(command =>
                $"Priority library item is not a {LibraryType.PUNCHLIST_PRIORITY}! " +
                $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.PriorityGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.PriorityGuid)),
                ApplyConditionTo.CurrentValidator)

            // validate Sorting, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SortingGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Sorting library item does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.SortingGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                command.PatchDocument.Operations,
                nameof(PatchablePunchItem.SortingGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SortingGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Sorting library item is voided! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.SortingGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SortingGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SortingGuid),
                    LibraryType.PUNCHLIST_SORTING,
                    cancellationToken))
            .WithMessage(command =>
                $"Sorting library item is not a {LibraryType.PUNCHLIST_SORTING}! " +
                $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.SortingGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SortingGuid)),
                ApplyConditionTo.CurrentValidator)

            // validate Type, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.TypeGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Type library item does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.TypeGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                command.PatchDocument.Operations,
                nameof(PatchablePunchItem.TypeGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.TypeGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Type library item is voided! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.TypeGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.TypeGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.TypeGuid),
                    LibraryType.PUNCHLIST_TYPE,
                    cancellationToken))
            .WithMessage(command =>
                $"Type library item is not a {LibraryType.PUNCHLIST_TYPE}! " +
                $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.TypeGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.TypeGuid)),
                ApplyConditionTo.CurrentValidator);

        async Task<bool> NotBeInAClosedProjectForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.ProjectOwningPunchItemIsClosedAsync(punchItemGuid, cancellationToken);

        async Task<bool> NotBeInAVoidedTagForPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => !await punchItemValidator.TagOwningPunchItemIsVoidedAsync(punchItemGuid, cancellationToken);

        async Task<bool> BeAnExistingPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);

        Guid GetGuidValue(List<Operation<PatchablePunchItem>> operations, string propName)
        {
            // due to validation, the operation for propName SHOULD exists ...
            var operation = GetOperation(operations, propName)!;
            // ...  and the value it SHOULD be a Guid ...
            if (operation.value is Guid guid)
            {
                return guid;
            }
            // ... or can be parsed to Guid
            return Guid.Parse((string)operation.value);
        }

        bool ValueIsReplacedWithGuid(List<Operation<PatchablePunchItem>> operations, string propName)
        {
            var operation = GetOperation(operations, propName);
            var b = operation is not null
                    && (operation.value is Guid || Guid.TryParse(operation.value as string, out _));
            return b;
        }

        async Task<bool> BeAnExistingLibraryItemAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return await libraryItemValidator.ExistsAsync(guid, cancellationToken);
        }

        async Task<bool> BeALibraryItemOfTypeAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            LibraryType type,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return await libraryItemValidator.HasTypeAsync(guid, type, cancellationToken);
        }

        async Task<bool> NotBeAVoidedLibraryItemAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return !await libraryItemValidator.IsVoidedAsync(guid, cancellationToken);
        }

        Operation<PatchablePunchItem>? GetOperation(
            List<Operation<PatchablePunchItem>> operations,
            string propName)
            => operations.SingleOrDefault(op =>
                op.OperationType == OperationType.Replace && op.path == $"/{propName}");
    }
}
