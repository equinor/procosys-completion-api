using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;

public class DuplicatePunchItemCommandHandler(
    IPunchItemRepository punchItemRepository,
    IAttachmentRepository attachmentRepository,
    IAttachmentService attachmentService,
    ISyncToPCS4Service syncToPCS4Service,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ICheckListApiService checkListApiService,
    ILogger<DuplicatePunchItemCommandHandler> logger
    )
    : PunchUpdateCommandBase, IRequestHandler<DuplicatePunchItemCommand, Result<List<GuidAndRowVersion>>>
{
    public async Task<Result<List<GuidAndRowVersion>>> Handle(DuplicatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;
        var attachments = 
            request.DuplicateAttachments ? 
                (await attachmentRepository.GetAllByParentGuidAsync(request.PunchItemGuid, cancellationToken)).ToList() : [];

        var punchCopiesAndProperties =
            request.CheckListGuids.Select(checkListGuid => CreatePunchCopyAndBuildHistoryProperties(punchItem, checkListGuid))
                .ToList();

        var punchItemIntegrationEvents = new List<PunchItemCreatedIntegrationEvent>();
        var attachmentIntegrationEvents = new List<AttachmentCreatedIntegrationEvent>();

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        var checkListGuidsAsString = string.Join(",", request.CheckListGuids);
        try
        {
            foreach (var (punchCopy, punchCopyProperties) in punchCopiesAndProperties)
            {
                punchItemRepository.Add(punchCopy);

                // must save twice when creating. Must save before publishing events both to set with internal database ID
                // since ItemNo depend on it. Must save after publishing events because we use outbox pattern
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // Add property for ItemNo first in list, since it is an "important" property
                punchCopyProperties.Insert(0, new Property(nameof(PunchItem.ItemNo), punchItem.ItemNo, ValueDisplayType.IntAsText));

                var integrationEvent = await PublishPunchItemCreatedIntegrationEventsAsync(
                    punchCopy.ItemNo, 
                    punchCopy,
                    punchCopyProperties, 
                    cancellationToken);
                punchItemIntegrationEvents.Add(integrationEvent);

                if (request.DuplicateAttachments)
                {
                    // copy attachments and collect attachment copied events.
                    var attachmentEvents = await attachmentService.CopyAttachments(
                        attachments, 
                        nameof(PunchItem),
                        punchCopy.Guid, 
                        punchCopy.Project.Name,
                        cancellationToken);
                    attachmentIntegrationEvents.AddRange(attachmentEvents);
                }
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} duplicated to check lists {CheckListGuids}", 
                punchItem.ItemNo,
                punchItem.Guid,
                checkListGuidsAsString);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred on duplication of PunchListItem");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var guidAndRowVersions = punchCopiesAndProperties.Select(
            x => new GuidAndRowVersion(x.Item1.Guid, x.Item1.RowVersion.ConvertToString())).ToList();

        try
        {
            await Task.WhenAll(punchItemIntegrationEvents.Select(integrationEvent =>
                syncToPCS4Service.SyncNewPunchListItemAsync(integrationEvent, cancellationToken)));
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error occurred while trying to Sync Duplicated punch items with guids {PunchItemGuids}",
                string.Join(",", punchItemIntegrationEvents.Select(i => i.Guid)));
            return new SuccessResult<List<GuidAndRowVersion>>(guidAndRowVersions);
        }

        try
        {
            await checkListApiService.RecalculateCheckListStatusForMany(punchItem.Plant, request.CheckListGuids, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error occurred while trying to Recalculate the completion status for check lists with guids {CheckListGuids}",
                checkListGuidsAsString);
            return new SuccessResult<List<GuidAndRowVersion>>(guidAndRowVersions);
        }

        try
        {
            await Task.WhenAll(attachmentIntegrationEvents.Select(integrationEvent =>
                syncToPCS4Service.SyncNewAttachmentAsync(integrationEvent, cancellationToken)));
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error occurred while trying to Sync Duplicated attachments with guids {AttachmentGuids}",
                string.Join(",", attachmentIntegrationEvents.Select(i => i.Guid)));
        }

        return new SuccessResult<List<GuidAndRowVersion>>(guidAndRowVersions);
    }

    private async Task<PunchItemCreatedIntegrationEvent> PublishPunchItemCreatedIntegrationEventsAsync(
        long sourceItemNo,
        PunchItem punchItem,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemCreatedIntegrationEvent(punchItem);
        await messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryCreatedIntegrationEvent(
            $"Punch item {punchItem.Category} {punchItem.ItemNo} duplicated from {sourceItemNo}",
            punchItem.Guid,
            punchItem.CheckListGuid,
            new User(punchItem.CreatedBy.Guid, punchItem.CreatedBy.GetFullName()),
            punchItem.CreatedAtUtc,
            properties);
        await messageProducer.SendHistoryAsync(historyEvent, cancellationToken);

        return integrationEvent;
    }

    private static (PunchItem, List<IProperty>) CreatePunchCopyAndBuildHistoryProperties(PunchItem sourcePunchItem, Guid checkListGuid)
    {
        var newPunchItem = new PunchItem(
            sourcePunchItem.Plant,
            sourcePunchItem.Project,
            checkListGuid,
            sourcePunchItem.Category,
            sourcePunchItem.Description,
            sourcePunchItem.RaisedByOrg,
            sourcePunchItem.ClearingByOrg);

        var properties = newPunchItem.GetRequiredProperties();

        SetActionBy(newPunchItem, sourcePunchItem.ActionBy, properties);
        SetDueTime(newPunchItem, sourcePunchItem.DueTimeUtc, properties);
        SetLibraryItem(newPunchItem, sourcePunchItem.Priority, LibraryType.COMM_PRIORITY, properties);
        SetLibraryItem(newPunchItem, sourcePunchItem.Sorting, LibraryType.PUNCHLIST_SORTING, properties);
        SetLibraryItem(newPunchItem, sourcePunchItem.Type, LibraryType.PUNCHLIST_TYPE, properties);
        SetEstimate(newPunchItem, sourcePunchItem.Estimate, properties);
        SetOriginalWorkOrder(newPunchItem, sourcePunchItem.OriginalWorkOrder, properties);
        SetWorkOrder(newPunchItem, sourcePunchItem.WorkOrder, properties);
        SetSWCR(newPunchItem, sourcePunchItem.SWCR, properties);
        SetDocument(newPunchItem, sourcePunchItem.Document, properties);
        SetExternalItemNo(newPunchItem, sourcePunchItem.ExternalItemNo, properties);
        SetMaterialRequired(newPunchItem, sourcePunchItem.MaterialRequired, properties);
        SetMaterialETAUtc(newPunchItem, sourcePunchItem.MaterialETAUtc, properties);
        SetMaterialExternalNo(newPunchItem, sourcePunchItem.MaterialExternalNo, properties);

        return (newPunchItem, properties);
    }

    private static void SetActionBy(
        PunchItem punchItem,
        Person? actionByPerson,
        ICollection<IProperty> properties)
    {
        if (actionByPerson is null)
        {
            return;
        }

        punchItem.SetActionBy(actionByPerson);
        properties.Add(new Property(
            nameof(PunchItem.ActionBy),
            new User(punchItem.ActionBy!.Guid, punchItem.ActionBy!.GetFullName()),
            ValueDisplayType.UserAsNameOnly));
    }

    private static void SetDueTime(PunchItem punchItem, DateTime? dueTimeUtc, ICollection<IProperty> properties)
    {
        if (dueTimeUtc is null)
        {
            return;
        }

        punchItem.DueTimeUtc = dueTimeUtc;
        properties.Add(new Property(nameof(PunchItem.DueTimeUtc), punchItem.DueTimeUtc.Value,
            ValueDisplayType.DateTimeAsDateOnly));
    }

    private static void SetLibraryItem(
        PunchItem punchItem,
        LibraryItem? libraryItem,
        LibraryType libraryType,
        ICollection<IProperty> properties)
    {
        if (libraryItem is null)
        {
            return;
        }

        switch (libraryType)
        {
            case LibraryType.COMM_PRIORITY:
                punchItem.SetPriority(libraryItem);
                properties.Add(new Property(nameof(PunchItem.Priority), punchItem.Priority!.ToString()));
                break;
            case LibraryType.PUNCHLIST_SORTING:
                punchItem.SetSorting(libraryItem);
                properties.Add(new Property(nameof(PunchItem.Sorting), punchItem.Sorting!.ToString()));
                break;
            case LibraryType.PUNCHLIST_TYPE:
                punchItem.SetType(libraryItem);
                properties.Add(new Property(nameof(PunchItem.Type), punchItem.Type!.ToString()));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }
    }

    private static void SetEstimate(PunchItem punchItem, int? estimate, ICollection<IProperty> properties)
    {
        punchItem.Estimate = estimate;
        if (estimate is null)
        {
            return;
        }

        properties.Add(new Property(nameof(PunchItem.Estimate), estimate.Value, ValueDisplayType.IntAsText));
    }

    private static void SetOriginalWorkOrder(
        PunchItem punchItem,
        WorkOrder? originalWorkOrder,
        ICollection<IProperty> properties)
    {
        if (originalWorkOrder is null)
        {
            return;
        }

        punchItem.SetOriginalWorkOrder(originalWorkOrder);
        properties.Add(new Property(nameof(PunchItem.OriginalWorkOrder), punchItem.OriginalWorkOrder!.No));
    }

    private static void SetWorkOrder(
        PunchItem punchItem,
        WorkOrder? workOrder,
        ICollection<IProperty> properties)
    {
        if (workOrder is null)
        {
            return;
        }

        punchItem.SetWorkOrder(workOrder);
        properties.Add(new Property(nameof(PunchItem.WorkOrder), punchItem.WorkOrder!.No));
    }

    private static void SetSWCR(
        PunchItem punchItem,
        SWCR? swcr,
        ICollection<IProperty> properties)
    {
        if (swcr is null)
        {
            return;
        }

        punchItem.SetSWCR(swcr);
        properties.Add(new Property(nameof(PunchItem.SWCR), punchItem.SWCR!.No, ValueDisplayType.IntAsText));
    }

    private static void SetDocument(
        PunchItem punchItem,
        Document? document,
        ICollection<IProperty> properties)
    {
        if (document is null)
        {
            return;
        }

        punchItem.SetDocument(document);
        properties.Add(new Property(nameof(PunchItem.Document), punchItem.Document!.No));
    }

    private static void SetExternalItemNo(PunchItem punchItem, string? externalItemNo, ICollection<IProperty> properties)
    {
        if (externalItemNo is null)
        {
            return;
        }

        punchItem.ExternalItemNo = externalItemNo;
        properties.Add(new Property(nameof(PunchItem.ExternalItemNo), punchItem.ExternalItemNo));
    }

    private static void SetMaterialRequired(PunchItem punchItem, bool materialRequired, ICollection<IProperty> properties)
    {
        if (!materialRequired)
        {
            return;
        }

        punchItem.MaterialRequired = materialRequired;
        properties.Add(new Property(nameof(PunchItem.MaterialRequired), punchItem.MaterialRequired,
            ValueDisplayType.BoolAsYesNo));
    }

    private static void SetMaterialETAUtc(PunchItem punchItem, DateTime? materialETAUtc, ICollection<IProperty> properties)
    {
        if (materialETAUtc is null)
        {
            return;
        }

        punchItem.MaterialETAUtc = materialETAUtc;
        properties.Add(new Property(nameof(PunchItem.MaterialETAUtc), punchItem.MaterialETAUtc,
            ValueDisplayType.DateTimeAsDateOnly));
    }

    private static void SetMaterialExternalNo(PunchItem punchItem, string? materialExternalNo, ICollection<IProperty> properties)
    {
        if (materialExternalNo is null)
        {
            return;
        }

        punchItem.MaterialExternalNo = materialExternalNo;
        properties.Add(new Property(nameof(PunchItem.MaterialExternalNo), punchItem.MaterialExternalNo));
    }
}
