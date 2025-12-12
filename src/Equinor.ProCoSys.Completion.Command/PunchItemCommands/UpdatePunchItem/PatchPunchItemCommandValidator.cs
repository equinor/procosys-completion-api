using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

/// <summary>
/// Generic validator for commands that patch a PunchItem.
/// Validates the patch document operations for both UpdatePunchItemCommand and ImportUpdatePunchItemCommand.
/// </summary>
public class PatchPunchItemCommandValidator<T> : AbstractValidator<T> where T : IPatchPunchItemCommand
{
    // Business Validation is based on that Input Validation is done in advance, thus all replaced ...
    // ... guid values are validated to be Guids
    public PatchPunchItemCommandValidator(
        ILibraryItemValidator libraryItemValidator,
        IWorkOrderValidator workOrderValidator,
        ISWCRValidator swcrValidator,
        IDocumentValidator documentValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
            .WithMessage("Tag owning punch item is voided!")
             .Must(command => !command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item is cleared! Guid={command.PunchItemGuid}")
            .When(command => command.PatchDocument.Operations.Count > 0)

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
                    LibraryType.COMM_PRIORITY,
                    cancellationToken))
            .WithMessage(command =>
                $"Priority library item is not a {LibraryType.COMM_PRIORITY}! " +
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
                ApplyConditionTo.CurrentValidator)

            // validate OriginalWorkOrder, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingWorkOrderAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.OriginalWorkOrderGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Original WO does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.OriginalWorkOrderGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.OriginalWorkOrderGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAClosedWorkOrderAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.OriginalWorkOrderGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Original WO is closed! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.OriginalWorkOrderGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.OriginalWorkOrderGuid)),
                ApplyConditionTo.CurrentValidator)

            // validate WorkOrder, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingWorkOrderAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.WorkOrderGuid),
                    cancellationToken))
            .WithMessage(command
                => $"WO does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.WorkOrderGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.WorkOrderGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAClosedWorkOrderAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.WorkOrderGuid),
                    cancellationToken))
            .WithMessage(command
                => $"WO is closed! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.WorkOrderGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.WorkOrderGuid)),
                ApplyConditionTo.CurrentValidator)

            // validate SWCR, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingSWCRAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SWCRGuid),
                    cancellationToken))
            .WithMessage(command
                => $"SWCR does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.SWCRGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SWCRGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedSWCRAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SWCRGuid),
                    cancellationToken))
            .WithMessage(command
                => $"SWCR is voided! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.SWCRGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.SWCRGuid)),
                ApplyConditionTo.CurrentValidator)

            // validate Document, if given
            .MustAsync((command, cancellationToken)
                => BeAnExistingDocumentAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.DocumentGuid), 
                    cancellationToken))
            .WithMessage(command
                => $"Document does not exist! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.DocumentGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.DocumentGuid)),
                ApplyConditionTo.CurrentValidator)
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedDocumentAsync(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.DocumentGuid),
                    cancellationToken))
            .WithMessage(command
                => $"Document is voided! " +
                   $"Guid={GetGuidValue(command.PatchDocument.Operations, nameof(PatchablePunchItem.DocumentGuid))}")
            .When(command => ValueIsReplacedWithGuid(
                    command.PatchDocument.Operations,
                    nameof(PatchablePunchItem.DocumentGuid)),
                ApplyConditionTo.CurrentValidator);

        Guid GetGuidValue(List<Operation<PatchablePunchItem>> operations, string propName)
        {
            // due to already performed input validation, the operation for propName SHOULD exists ...
            var operation = operations.GetReplaceOperation(propName)!;
            // ...  and the value SHOULD be either a Guid ...
            if (operation.value is Guid guid)
            {
                return guid;
            }
            // ... or the value can be parsed to Guid
            return Guid.Parse((string)operation.value);
        }

        bool ValueIsReplacedWithGuid(List<Operation<PatchablePunchItem>> operations, string propName)
        {
            var operation = operations.GetReplaceOperation(propName);
            // due to already performed input validation, the operation for replacing a Guid or Guid?
            // are validated to be treated as a Guid.
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

        async Task<bool> BeAnExistingWorkOrderAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return await workOrderValidator.ExistsAsync(guid, cancellationToken);
        }

        async Task<bool> NotBeAClosedWorkOrderAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return !await workOrderValidator.IsVoidedAsync(guid, cancellationToken);
        }

        async Task<bool> BeAnExistingSWCRAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return await swcrValidator.ExistsAsync(guid, cancellationToken);
        }

        async Task<bool> NotBeAVoidedSWCRAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return !await swcrValidator.IsVoidedAsync(guid, cancellationToken);
        }

        async Task<bool> BeAnExistingDocumentAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return await documentValidator.ExistsAsync(guid, cancellationToken);
        }

        async Task<bool> NotBeAVoidedDocumentAsync(
            List<Operation<PatchablePunchItem>> operations,
            string propName,
            CancellationToken cancellationToken)
        {
            var guid = GetGuidValue(operations, propName);
            return !await documentValidator.IsVoidedAsync(guid, cancellationToken);
        }
    }
}
