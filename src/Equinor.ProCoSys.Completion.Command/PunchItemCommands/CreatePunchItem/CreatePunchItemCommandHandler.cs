﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommandHandler<TRequest>(
    IPlantProvider plantProvider,
    IPunchItemRepository punchItemRepository,
    ILibraryItemRepository libraryItemRepository,
    IProjectRepository projectRepository,
    IPersonRepository personRepository,
    IWorkOrderRepository woRepository,
    ISWCRRepository swcrRepository,
    IDocumentRepository documentRepository,
    ISyncToPCS4Service syncToPCS4Service,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ICheckListApiService checkListApiService,
    ILogger<TRequest> logger)
    : IRequestHandler<TRequest, GuidAndRowVersion>
    where TRequest : CreatePunchItemCommand
{
    public async Task<GuidAndRowVersion> Handle(TRequest request,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetAsync(request.CheckListDetailsDto.ProjectGuid, cancellationToken);

        var raisedByOrg = await libraryItemRepository.GetByGuidAndTypeAsync(
            request.RaisedByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);
        var clearingByOrg = await libraryItemRepository.GetByGuidAndTypeAsync(
            request.ClearingByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);

        var punchItem = new PunchItem(
            plantProvider.Plant,
            project,
            request.CheckListGuid,
            request.Category,
            request.Description,
            raisedByOrg,
            clearingByOrg);

        var properties = punchItem.GetRequiredProperties();

        await SetActionByAsync(punchItem, request.ActionByPersonOid, properties, cancellationToken);
        SetDueTime(punchItem, request.DueTimeUtc, properties);
        await SetLibraryItemAsync(punchItem, request.PriorityGuid, LibraryType.COMM_PRIORITY, properties,
            cancellationToken);
        await SetLibraryItemAsync(punchItem, request.SortingGuid, LibraryType.PUNCHLIST_SORTING, properties,
            cancellationToken);
        await SetLibraryItemAsync(punchItem, request.TypeGuid, LibraryType.PUNCHLIST_TYPE, properties,
            cancellationToken);
        SetEstimate(punchItem, request.Estimate, properties);
        await SetOriginalWorkOrderAsync(punchItem, request.OriginalWorkOrderGuid, properties, cancellationToken);
        await SetWorkOrderAsync(punchItem, request.WorkOrderGuid, properties, cancellationToken);
        await SetSWCRAsync(punchItem, request.SWCRGuid, properties, cancellationToken);
        await SetDocumentAsync(punchItem, request.DocumentGuid, properties, cancellationToken);
        SetExternalItemNo(punchItem, request.ExternalItemNo, properties);
        SetMaterialRequired(punchItem, request.MaterialRequired, properties);
        SetMaterialETAUtc(punchItem, request.MaterialETAUtc, properties);
        SetMaterialExternalNo(punchItem, request.MaterialExternalNo, properties);

        PunchItemCreatedIntegrationEvent integrationEvent;
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            punchItemRepository.Add(punchItem);

            // must save twice when creating. Must save before publishing events both to set with internal database ID
            // since ItemNo depend on it. Must save after publishing events because we use outbox pattern
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Add property for ItemNo first in list, since it is an "important" property
            properties.Insert(0, new Property(nameof(PunchItem.ItemNo), punchItem.ItemNo, ValueDisplayType.IntAsText));

            integrationEvent = await PublishPunchItemCreatedIntegrationEventsAsync(punchItem, properties, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} created", punchItem.ItemNo,
                punchItem.Guid);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred on insertion of PunchListItem");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var guidAndRowVersion = new GuidAndRowVersion(punchItem.Guid,
            punchItem.RowVersion.ConvertToString());
        try
        {
            await syncToPCS4Service.SyncNewPunchListItemAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, 
                "Error occurred while trying to Sync Create on PunchItemList with guid {PunchItemGuid}", 
                punchItem.Guid);
            return guidAndRowVersion;
        }

        try
        {
            await checkListApiService.RecalculateCheckListStatusAsync(punchItem.Plant, punchItem.CheckListGuid, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error occurred while trying to Recalculate the completion status for check list with guid {CheckListGuid}", 
                punchItem.CheckListGuid);
        }

        return guidAndRowVersion;
    }

    private async Task<PunchItemCreatedIntegrationEvent> PublishPunchItemCreatedIntegrationEventsAsync(
        PunchItem punchItem,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemCreatedIntegrationEvent(punchItem);
        await messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryCreatedIntegrationEvent(
            $"Punch item {punchItem.Category} {punchItem.ItemNo} created",
            punchItem.Guid,
            punchItem.CheckListGuid,
            new User(punchItem.CreatedBy.Guid, punchItem.CreatedBy.GetFullName()),
            punchItem.CreatedAtUtc,
            properties);
        await messageProducer.SendHistoryAsync(historyEvent, cancellationToken);

        return integrationEvent;
    }

    private void SetMaterialExternalNo(PunchItem punchItem, string? materialExternalNo, List<IProperty> properties)
    {
        if (materialExternalNo is null)
        {
            return;
        }

        punchItem.MaterialExternalNo = materialExternalNo;
        properties.Add(new Property(nameof(PunchItem.MaterialExternalNo), punchItem.MaterialExternalNo));
    }

    private void SetMaterialETAUtc(PunchItem punchItem, DateTime? materialETAUtc, List<IProperty> properties)
    {
        if (materialETAUtc is null)
        {
            return;
        }

        punchItem.MaterialETAUtc = materialETAUtc;
        properties.Add(new Property(nameof(PunchItem.MaterialETAUtc), punchItem.MaterialETAUtc,
            ValueDisplayType.DateTimeAsDateOnly));
    }

    private void SetMaterialRequired(PunchItem punchItem, bool materialRequired, List<IProperty> properties)
    {
        if (!materialRequired)
        {
            return;
        }

        punchItem.MaterialRequired = materialRequired;
        properties.Add(new Property(nameof(PunchItem.MaterialRequired), punchItem.MaterialRequired,
            ValueDisplayType.BoolAsYesNo));
    }

    private void SetExternalItemNo(PunchItem punchItem, string? externalItemNo, List<IProperty> properties)
    {
        if (externalItemNo is null)
        {
            return;
        }

        punchItem.ExternalItemNo = externalItemNo;
        properties.Add(new Property(nameof(PunchItem.ExternalItemNo), punchItem.ExternalItemNo));
    }

    private void SetEstimate(PunchItem punchItem, int? estimate, List<IProperty> properties)
    {
        punchItem.Estimate = estimate;
        if (estimate is null)
        {
            return;
        }

        properties.Add(new Property(nameof(PunchItem.Estimate), estimate.Value, ValueDisplayType.IntAsText));
    }

    private void SetDueTime(PunchItem punchItem, DateTime? dueTimeUtc, List<IProperty> properties)
    {
        if (dueTimeUtc is null)
        {
            return;
        }

        punchItem.DueTimeUtc = dueTimeUtc;
        properties.Add(new Property(nameof(PunchItem.DueTimeUtc), punchItem.DueTimeUtc.Value,
            ValueDisplayType.DateTimeAsDateOnly));
    }

    private async Task SetDocumentAsync(
        PunchItem punchItem,
        Guid? documentGuid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (documentGuid is null)
        {
            return;
        }

        var doc = await documentRepository.GetAsync(documentGuid.Value, cancellationToken);
        punchItem.SetDocument(doc);
        properties.Add(new Property(nameof(PunchItem.Document), punchItem.Document!.No));
    }

    private async Task SetSWCRAsync(
        PunchItem punchItem,
        Guid? swcrGuid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (swcrGuid is null)
        {
            return;
        }

        var swcr = await swcrRepository.GetAsync(swcrGuid.Value, cancellationToken);
        punchItem.SetSWCR(swcr);
        properties.Add(new Property(nameof(PunchItem.SWCR), punchItem.SWCR!.No, ValueDisplayType.IntAsText));
    }

    private async Task SetOriginalWorkOrderAsync(
        PunchItem punchItem,
        Guid? originalWorkOrderGuid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (originalWorkOrderGuid is null)
        {
            return;
        }

        var wo = await woRepository.GetAsync(originalWorkOrderGuid.Value, cancellationToken);
        punchItem.SetOriginalWorkOrder(wo);
        properties.Add(new Property(nameof(PunchItem.OriginalWorkOrder), punchItem.OriginalWorkOrder!.No));
    }

    private async Task SetWorkOrderAsync(
        PunchItem punchItem,
        Guid? workOrderGuid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (workOrderGuid is null)
        {
            return;
        }

        var wo = await woRepository.GetAsync(workOrderGuid.Value, cancellationToken);
        punchItem.SetWorkOrder(wo);
        properties.Add(new Property(nameof(PunchItem.WorkOrder), punchItem.WorkOrder!.No));
    }

    private async Task SetActionByAsync(
        PunchItem punchItem,
        Guid? actionByPersonOid,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (actionByPersonOid is null)
        {
            return;
        }

        var person = await personRepository.GetOrCreateAsync(actionByPersonOid.Value, cancellationToken);
        punchItem.SetActionBy(person);
        properties.Add(new Property(
            nameof(PunchItem.ActionBy),
            new User(punchItem.ActionBy!.Guid, punchItem.ActionBy!.GetFullName()),
            ValueDisplayType.UserAsNameOnly));
    }

    private async Task SetLibraryItemAsync(
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType,
        List<IProperty> properties,
        CancellationToken cancellationToken)
    {
        if (libraryGuid is null)
        {
            return;
        }

        var libraryItem =
            await libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid.Value, libraryType, cancellationToken);

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
}
