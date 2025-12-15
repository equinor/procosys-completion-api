using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;

public sealed class ImportUpdatePunchItemCommandHandler(
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ILogger<ImportUpdatePunchItemCommandHandler> logger,
    ISyncToPCS4Service syncToPCS4Service,
    ICheckListApiService checkListApiService,
    ILibraryItemRepository libraryItemRepository,
    IWorkOrderRepository workOrderRepository,
    ISWCRRepository swcrRepository,
    IDocumentRepository documentRepository)
    : PunchUpdateCommandBase, IRequestHandler<ImportUpdatePunchItemCommand, List<ImportError>>
{
    private const string RejectReasonPropertyName = "Reject reason";
    
    private ImportError ToImportError(ImportUpdatePunchItemCommand request, string message) =>
        new(
            request.ImportGuid,
            "UPDATE",
            $"Project with GUID {request.ProjectGuid}",
            request.Plant,
            message);

    private static bool IsGeneralUpdate(ImportUpdatePunchItemCommand request) => request.PatchDocument.Operations.Count != 0;

    private static bool IsRejectingPunch(ImportUpdatePunchItemCommand request) => request.RejectedBy.HasValue;

    private static bool IsVerifyingPunch(ImportUpdatePunchItemCommand request) => request.VerifiedBy.HasValue;

    private static bool IsClearingPunch(ImportUpdatePunchItemCommand request) => request.ClearedBy.HasValue;

    public async Task<List<ImportError>> Handle(ImportUpdatePunchItemCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<ImportError>();
        var events = new List<object>();
        var punchItem = request.PunchItem;

        // Mark as modified and set audit data BEFORE patching, 
        // because PublishPunchItemUpdatedIntegrationEventsAsync requires ModifiedBy to be set
        unitOfWork.SetModified(punchItem);
        await unitOfWork.SetAuditDataAsync();

        errors.AddRange(await PatchPunchAsync(request, punchItem, events, cancellationToken));

        punchItem.SetRowVersion(request.RowVersion);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Punch item update import failed to commit changes. Import Guid: '{Guid}'",
                request.ImportGuid);
            errors.Add(ToImportError(request, ex.Message));

            // Do not continue with sync and checklist recalculation if save failed
            return errors;
        }

        try
        {
            var syncTasks = events.Select(x => syncToPCS4Service.SyncPunchListItemUpdateAsync(x, cancellationToken));
            await Task.WhenAll(syncTasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Punch item update import failed to sync to PCS4, PunchItem with guid {PunchItemGuid}",
                request.PunchItemGuid);
            errors.Add(ToImportError(request, e.Message));
            return errors;
        }

        try
        {
            await checkListApiService.RecalculateCheckListStatusAsync(punchItem.Plant, punchItem.CheckListGuid,
                cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Punch item update import failed to Recalculate the CheckListStatus for CheckList with Guid {Guid}",
                punchItem.CheckListGuid);
            errors.Add(ToImportError(request, e.Message));
        }

        return errors;
    }

    private async Task<ImportError[]> PatchPunchAsync(ImportUpdatePunchItemCommand request,
        PunchItem punchItem, List<object> events, CancellationToken cancellationToken)
    {
        ImportError[] errors = [];
        if (IsClearingPunch(request))
        {
            var clearEvent = await HandleClearAsync(request, punchItem, cancellationToken);
            if (clearEvent is ImportError e)
            {
                errors = [..errors, e];
            }
            else
            {
                events.Add(clearEvent);
            }
        }

        if (IsVerifyingPunch(request))
        {
            var verifyEvent = await HandleVerifyAsync(request, punchItem, cancellationToken);
            if (verifyEvent is ImportError e)
            {
                errors = [..errors, e];
            }
            else
            {
                events.Add(verifyEvent);
            }
        }

        if (IsRejectingPunch(request))
        {
            var rejectEvent = await HandleRejectAsync(request, punchItem, cancellationToken);
            if (rejectEvent is ImportError e)
            {
                errors = [..errors, e];
            }
            else
            {
                events.Add(rejectEvent);
            }
        }

        if (IsGeneralUpdate(request))
        {
            var updateEvent = await HandleUpdateAsync(request, punchItem, cancellationToken);
            if (updateEvent is ImportError error)
            {
                errors = [..errors, error];
            }
            else if (updateEvent is PunchItemUpdatedIntegrationEvent)
            {
                events.Add(updateEvent);
            }
        }

        return errors;
    }

    private async Task<object?> HandleUpdateAsync(ImportUpdatePunchItemCommand request, PunchItem punchItem, CancellationToken cancellationToken)
    {
        var changes = new List<IChangedProperty>();

        var propertiesToReplace = PunchItemPatcher.GetPropertiesToReplace(request.PatchDocument);
        if (!propertiesToReplace.Any())
        {
            return null;
        }

        var patchedPunchItem = new PatchablePunchItem();

        request.PatchDocument.ApplyTo(patchedPunchItem);

        foreach (var propertyToReplace in propertiesToReplace)
        {
            switch (propertyToReplace)
            {
                case nameof(PatchablePunchItem.Category):
                    PunchItemPatcher.PatchCategory(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.Description):
                    PunchItemPatcher.PatchDescription(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.RaisedByOrgGuid):
                    await PunchItemPatcher.PatchRaisedByOrgAsync(punchItem, patchedPunchItem, changes, libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.ClearingByOrgGuid):
                    await PunchItemPatcher.PatchClearingByOrgGuidAsync(punchItem, patchedPunchItem, changes, libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.ActionByPersonOid):
                    await PunchItemPatcher.PatchActionByPersonAsync(punchItem, patchedPunchItem, changes, personRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.DueTimeUtc):
                    PunchItemPatcher.PatchDueTime(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.Estimate):
                    PunchItemPatcher.PatchEstimate(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.PriorityGuid):
                    await PunchItemPatcher.PatchPriorityAsync(punchItem, patchedPunchItem, changes, libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.SortingGuid):
                    await PunchItemPatcher.PatchSortingAsync(punchItem, patchedPunchItem, changes, libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.TypeGuid):
                    await PunchItemPatcher.PatchTypeAsync(punchItem, patchedPunchItem, changes, libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.OriginalWorkOrderGuid):
                    await PunchItemPatcher.PatchOriginalWorkOrderAsync(punchItem, patchedPunchItem, changes, workOrderRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.WorkOrderGuid):
                    await PunchItemPatcher.PatchWorkOrderAsync(punchItem, patchedPunchItem, changes, workOrderRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.SWCRGuid):
                    await PunchItemPatcher.PatchSWCRAsync(punchItem, patchedPunchItem, changes, swcrRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.DocumentGuid):
                    await PunchItemPatcher.PatchDocumentAsync(punchItem, patchedPunchItem, changes, documentRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.MaterialRequired):
                    PunchItemPatcher.PatchMaterialRequired(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.MaterialETAUtc):
                    PunchItemPatcher.PatchMaterialETA(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.MaterialExternalNo):
                    PunchItemPatcher.PatchMaterialExternalNo(punchItem, patchedPunchItem, changes);
                    break;

                default:
                    throw new NotImplementedException($"Patching property {propertyToReplace} not implemented");
            }
        }

        if (changes.Count == 0)
        {
            return null;
        }

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            "Punch item updated",
            changes,
            cancellationToken);

        return integrationEvent;

    }

    private async Task<object> HandleRejectAsync(ImportUpdatePunchItemCommand request, PunchItem punchItem,
        CancellationToken cancellationToken)
    {
        var currentPerson = request.RejectedBy.Value!.Person;
        var change = new ChangedProperty<string?>(RejectReasonPropertyName, null, "IMPORTED REJECTION");

        punchItem.Reject(currentPerson, request.RejectedBy.Value?.ActionDate);

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            "Punch item rejected",
            [change],
            cancellationToken);
        
        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} rejected", punchItem.ItemNo,
            punchItem.Guid);

        return integrationEvent;
    }

    private async Task<object> HandleVerifyAsync(ImportUpdatePunchItemCommand request, PunchItem punchItem,
        CancellationToken cancellationToken)
    {
        var currentPerson = request.VerifiedBy.Value!.Person;
        punchItem.Verify(currentPerson, request.VerifiedBy.Value?.ActionDate);

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            "Punch item verified",
            [],
            cancellationToken);
        
        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} verified", punchItem.ItemNo, punchItem.Guid);

        return integrationEvent;
    }

    private async Task<object> HandleClearAsync(ImportUpdatePunchItemCommand request, PunchItem punchItem,
        CancellationToken cancellationToken)
    {
        
        var currentPerson = request.ClearedBy.Value!.Person;
        punchItem.Clear(currentPerson, request.ClearedBy.Value?.ActionDate);

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            "Punch item cleared",
            [],
            cancellationToken);
        
        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} cleared", punchItem.ItemNo, punchItem.Guid);

        return integrationEvent;
    }
}
