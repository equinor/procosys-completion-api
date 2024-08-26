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
        var attachments = (await attachmentRepository.GetAllByParentGuidAsync(request.PunchItemGuid, cancellationToken)).ToList();

        var punchCopiesAndProperties = request.CheckListGuids.Select(checkListGuid
            => CreatePunchCopy(punchItem, checkListGuid)).ToList();

        var integrationEvents = new List<PunchItemCreatedIntegrationEvent>();
        var attachmentIntegrationEvents = new List<AttachmentCreatedIntegrationEvent>();

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var (punchCopy, properties) in punchCopiesAndProperties)
            {
                punchItemRepository.Add(punchCopy);

                // must save twice when creating. Must save before publishing events both to set with internal database ID
                // since ItemNo depend on it. Must save after publishing events because we use outbox pattern
                await unitOfWork.SaveChangesAsync(cancellationToken);

                // Add property for ItemNo first in list, since it is an "important" property
                properties.Insert(0, new Property(nameof(PunchItem.ItemNo), punchItem.ItemNo, ValueDisplayType.IntAsText));

                var integrationEvent = await PublishPunchItemCreatedIntegrationEventsAsync(punchCopy.ItemNo, punchItem, properties, cancellationToken);
                integrationEvents.Add(integrationEvent);
                await messageProducer.PublishAsync(integrationEvent, cancellationToken);

                // copy attachments and collect attachment copied events.
                var attachmentEvents = await attachmentService.CopyAttachments(attachments, nameof(PunchItem), punchCopy.Guid, punchCopy.Project.Name,
                     cancellationToken);
                attachmentIntegrationEvents.AddRange(attachmentEvents);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred on insertion of PunchListItem");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        await unitOfWork.CommitTransactionAsync(cancellationToken);

        //TODO Sync to PCS 4 og recalc

        await Task.WhenAll(integrationEvents.Select(x =>
           syncToPCS4Service.SyncNewPunchListItemAsync(x, cancellationToken)));

        await Task.WhenAll(attachmentIntegrationEvents.Select(x =>
           syncToPCS4Service.SyncNewPunchListItemAsync(x, cancellationToken)));

        await Task.WhenAll(punchCopiesAndProperties.Select(x =>
            checkListApiService.RecalculateCheckListStatus(x.Item1.Plant, x.Item1.CheckListGuid, cancellationToken)));
        

        return new SuccessResult<List<GuidAndRowVersion>>(punchCopiesAndProperties.Select(
            x => new GuidAndRowVersion(x.Item1.Guid, x.Item1.RowVersion.ConvertToString())).ToList()
        );
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

    private static (PunchItem, List<IProperty>) CreatePunchCopy(PunchItem src, Guid checkListGuid)
    {
        var punchItem = new PunchItem(
            src.Plant,
            src.Project,
            checkListGuid,
            src.Category,
            src.Description,
            src.RaisedByOrg,
            src.ClearingByOrg);

        var properties = GetRequiredProperties(punchItem);

        SetActionBy(punchItem, src.ActionBy, properties);
        SetDueTime(punchItem, src.DueTimeUtc, properties);
        SetLibraryItem(punchItem, src.Priority, LibraryType.COMM_PRIORITY, properties);
        SetLibraryItem(punchItem, src.Sorting, LibraryType.PUNCHLIST_SORTING, properties);
        SetLibraryItem(punchItem, src.Type, LibraryType.PUNCHLIST_TYPE, properties);
        SetEstimate(punchItem, src.Estimate, properties);
        SetOriginalWorkOrder(punchItem, src.OriginalWorkOrder, properties);
        SetWorkOrder(punchItem, src.WorkOrder, properties);
        SetSWCR(punchItem, src.SWCR, properties);
        SetDocument(punchItem, src.Document, properties);
        SetExternalItemNo(punchItem, src.ExternalItemNo, properties);
        SetMaterialRequired(punchItem, src.MaterialRequired, properties);
        SetMaterialETAUtc(punchItem, src.MaterialETAUtc, properties);
        SetMaterialExternalNo(punchItem, src.MaterialExternalNo, properties);

        return (punchItem, properties);
    }

    private static List<IProperty> GetRequiredProperties(PunchItem punchItem)
        =>
        [
            new Property(nameof(PunchItem.Category), punchItem.Category.ToString()),
            new Property(nameof(PunchItem.Description), punchItem.Description),
            new Property(nameof(PunchItem.RaisedByOrg), punchItem.RaisedByOrg.Code),
            new Property(nameof(PunchItem.ClearingByOrg), punchItem.ClearingByOrg.Code)
        ];

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
