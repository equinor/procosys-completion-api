using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommandHandler : PunchUpdateCommandBase,
    IRequestHandler<UpdatePunchItemCommand, string>
{
    private readonly ILibraryItemRepository _libraryItemRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ISWCRRepository _swcrRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageProducer _messageProducer;
    private readonly ILogger<UpdatePunchItemCommandHandler> _logger;

    public UpdatePunchItemCommandHandler(
        ILibraryItemRepository libraryItemRepository,
        IPersonRepository personRepository,
        IWorkOrderRepository workOrderRepository,
        ISWCRRepository swcrRepository,
        IDocumentRepository documentRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IMessageProducer messageProducer,
        ILogger<UpdatePunchItemCommandHandler> logger)
    {
        _libraryItemRepository = libraryItemRepository;
        _personRepository = personRepository;
        _workOrderRepository = workOrderRepository;
        _swcrRepository = swcrRepository;
        _documentRepository = documentRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _logger = logger;
    }

    public async Task<string> Handle(UpdatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;

        var changes = await PatchAsync(punchItem, request.PatchDocument, cancellationToken);

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        IPunchItemUpdatedV1 integrationEvent = null!;
        if (changes.Count != 0)
        {
            integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
                _messageProducer,
                punchItem,
                "Punch item updated",
                changes,
                cancellationToken);
        }

        punchItem.SetRowVersion(request.RowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated", punchItem.ItemNo,
            punchItem.Guid);

        try
        {
            if (changes.Count != 0)
            {
                await _syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while trying to Sync Update on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
            return punchItem.RowVersion.ConvertToString();
        }

        return punchItem.RowVersion.ConvertToString();
    }

    private async Task<List<IChangedProperty>> PatchAsync(
        PunchItem punchItem,
        JsonPatchDocument<PatchablePunchItem> patchDocument,
        CancellationToken cancellationToken)
    {
        var changes = new List<IChangedProperty>();

        var propertiesToReplace = PunchItemPatcher.GetPropertiesToReplace(patchDocument);
        if (!propertiesToReplace.Any())
        {
            return changes;
        }

        var patchedPunchItem = new PatchablePunchItem();

        patchDocument.ApplyTo(patchedPunchItem);

        foreach (var propertyToReplace in propertiesToReplace)
        {
            switch (propertyToReplace)
            {
                case nameof(PatchablePunchItem.Description):
                    PunchItemPatcher.PatchDescription(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.RaisedByOrgGuid):
                    await PunchItemPatcher.PatchRaisedByOrgAsync(punchItem, patchedPunchItem, changes, _libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.ClearingByOrgGuid):
                    await PunchItemPatcher.PatchClearingByOrgGuidAsync(punchItem, patchedPunchItem, changes, _libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.ActionByPersonOid):
                    await PunchItemPatcher.PatchActionByPersonAsync(punchItem, patchedPunchItem, changes, _personRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.DueTimeUtc):
                    PunchItemPatcher.PatchDueTime(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.Estimate):
                    PunchItemPatcher.PatchEstimate(punchItem, patchedPunchItem, changes);
                    break;

                case nameof(PatchablePunchItem.PriorityGuid):
                    await PunchItemPatcher.PatchPriorityAsync(punchItem, patchedPunchItem, changes, _libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.SortingGuid):
                    await PunchItemPatcher.PatchSortingAsync(punchItem, patchedPunchItem, changes, _libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.TypeGuid):
                    await PunchItemPatcher.PatchTypeAsync(punchItem, patchedPunchItem, changes, _libraryItemRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.OriginalWorkOrderGuid):
                    await PunchItemPatcher.PatchOriginalWorkOrderAsync(punchItem, patchedPunchItem, changes, _workOrderRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.WorkOrderGuid):
                    await PunchItemPatcher.PatchWorkOrderAsync(punchItem, patchedPunchItem, changes, _workOrderRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.SWCRGuid):
                    await PunchItemPatcher.PatchSWCRAsync(punchItem, patchedPunchItem, changes, _swcrRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.DocumentGuid):
                    await PunchItemPatcher.PatchDocumentAsync(punchItem, patchedPunchItem, changes, _documentRepository, cancellationToken);
                    break;

                case nameof(PatchablePunchItem.ExternalItemNo):
                    PunchItemPatcher.PatchExternalItemNo(punchItem, patchedPunchItem, changes);
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

        return changes;
    }

    

    

    

    

    

    

    

    
    

    

    

    

    
}
