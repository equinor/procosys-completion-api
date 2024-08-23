using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
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
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;

public sealed record ImportUpdatePunchItemCommand(
    Guid ImportGuid,
    Guid ProjectGuid,
    string Plant,
    Guid PunchItemGuid,
    JsonPatchDocument<PatchablePunchItem> PatchDocument,
    Category? Category,
    Optional<ActionByPerson?> ClearedBy,
    Optional<ActionByPerson?> VerifiedBy,
    Optional<ActionByPerson?> RejectedBy,
    string RowVersion) : IRequest<Result<ImportError[]>>, IIsPunchItemCommand
{
    public PunchItem PunchItem { get; set; } = null!;
}

public sealed class ImportUpdatePunchItemHandler(
    ILabelValidator labelValidator,
    IOptionsMonitor<ApplicationOptions> options,
    ILibraryItemValidator libraryItemValidator,
    IWorkOrderValidator workOrderValidator,
    ISWCRValidator swcrValidator,
    IDocumentValidator documentValidator,
    IPunchItemRepository punchItemRepository,
    IPersonRepository personRepository,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ILogger<ImportUpdatePunchItemHandler> logger,
    ISyncToPCS4Service syncToPCS4Service,
    ICheckListApiService checkListApiService,
    ILibraryItemRepository libraryItemRepository,
    IWorkOrderRepository workOrderRepository,
    ISWCRRepository swcrRepository,
    IDocumentRepository documentRepository)
    : PunchUpdateCommandBase, IRequestHandler<ImportUpdatePunchItemCommand, Result<ImportError[]>>
{
    private const string RejectReasonPropertyName = "Reject reason";
    
    private ImportError ToImportError(ImportUpdatePunchItemCommand request, string message) =>
        new(
            request.ImportGuid,
            "UPDATE",
            $"Project with GUID {request.ProjectGuid}",
            request.Plant,
            message);

    private async Task<ImportError[]> Validate(ImportUpdatePunchItemCommand request,
        CancellationToken cancellationToken)
    {
        var clearValidator = new ClearPunchItemCommandValidator();
        var verifyValidator = new VerifyPunchItemCommandValidator();
        var rejectValidator = new RejectPunchItemCommandValidator( labelValidator, options);
        var updateValidator = new UpdatePunchItemCommandValidator( libraryItemValidator,
            workOrderValidator, swcrValidator, documentValidator);
        var categoryValidator = new UpdatePunchItemCategoryCommandValidator();

        var errors = new List<ImportError>();

        if (IsClearingPunch(request))
        {
            var results =
                await clearValidator.ValidateAsync(
                    new ClearPunchItemCommand(
                        request.PunchItemGuid,
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (IsVerifyingPunch(request))
        {
            var results =
                await verifyValidator.ValidateAsync(
                    new VerifyPunchItemCommand(
                        request.PunchItemGuid,
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (IsRejectingPunch(request))
        {
            var results =
                await rejectValidator.ValidateAsync(
                    new RejectPunchItemCommand(
                        request.PunchItemGuid,
                        string.Empty,
                        [],
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (IsUpdatingPunchCategory(request))
        {
            var results =
                await categoryValidator.ValidateAsync(
                    new UpdatePunchItemCategoryCommand(
                        request.PunchItemGuid,
                        request.Category!.Value,
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (IsGeneralUpdate(request))
        {
            var updateResults =
                await updateValidator.ValidateAsync(
                    new UpdatePunchItemCommand(
                        request.PunchItemGuid,
                        request.PatchDocument,
                        request.RowVersion),
                    cancellationToken);
            errors.AddRange(updateResults.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        return errors.ToArray();
    }

    private static bool IsGeneralUpdate(ImportUpdatePunchItemCommand request) => request.PatchDocument.Operations.Count != 0;

    private static bool IsUpdatingPunchCategory(ImportUpdatePunchItemCommand request) => request.Category.HasValue;

    private static bool IsRejectingPunch(ImportUpdatePunchItemCommand request) => request.RejectedBy.HasValue;

    private static bool IsVerifyingPunch(ImportUpdatePunchItemCommand request) => request.VerifiedBy.HasValue;

    private static bool IsClearingPunch(ImportUpdatePunchItemCommand request) => request.ClearedBy.HasValue;

    public async Task<Result<ImportError[]>> Handle(ImportUpdatePunchItemCommand request,
        CancellationToken cancellationToken)
    {
        var errors = await Validate(request, cancellationToken);
        var events = new List<object>();
        var punchItem = await punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

        await unitOfWork.SetAuditDataAsync();

        errors = [..errors, ..await PatchPunchAsync(request, punchItem, events, cancellationToken)];

        punchItem.SetRowVersion(request.RowVersion);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong when commiting import for Punch with Import Guid '{Guid}'",
                request.ImportGuid);
            errors = [..errors, ToImportError(request, ex.Message)];
        }

        try
        {
            var syncTasks = events.Select(x => syncToPCS4Service.SyncPunchListItemUpdateAsync(x, cancellationToken));
            await Task.WhenAll(syncTasks);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Sync Clear on PunchItemList with guid {PunchItemGuid}",
                request.PunchItemGuid);
            errors = [..errors, ToImportError(request, e.Message)];
            return new SuccessResult<ImportError[]>(errors);
        }

        try
        {
            await checkListApiService.RecalculateCheckListStatus(punchItem.Plant, punchItem.CheckListGuid,
                cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error occurred while trying to Recalculate the CheckListStatus for CheckList with Guid {Guid}",
                punchItem.CheckListGuid);
            errors = [..errors, ToImportError(request, e.Message)];
        }

        return new SuccessResult<ImportError[]>(errors);
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

            events.Add(clearEvent);
        }

        if (IsVerifyingPunch(request))
        {
            var verifyEvent = await HandleVerifyAsync(request, punchItem, cancellationToken);
            if (verifyEvent is ImportError e)
            {
                errors = [..errors, e];
            }

            events.Add(verifyEvent);
        }

        if (IsRejectingPunch(request))
        {
            var rejectEvent = await HandleRejectAsync(request, punchItem, cancellationToken);
            if (rejectEvent is ImportError e)
            {
                errors = [..errors, e];
            }

            events.Add(rejectEvent);
        }
        
        if (IsUpdatingPunchCategory(request))
        {
            var updateCategoryEvent = await HandleCategoryUpdateAsync(request, punchItem, cancellationToken);
            if (updateCategoryEvent is ImportError e)
            {
                errors = [..errors, e];
            }

            events.Add(updateCategoryEvent);
        }

        if (IsGeneralUpdate(request))
        {
            var updateEvent = await HandleUpdateAsync(request, punchItem, cancellationToken);
            if (updateEvent is ImportError error)
            {
                errors = [..errors, error];
            }

            if (updateEvent is PunchItemUpdatedIntegrationEvent)
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
            return changes;
        }

        var patchedPunchItem = new PatchablePunchItem();

        request.PatchDocument.ApplyTo(patchedPunchItem);

        foreach (var propertyToReplace in propertiesToReplace)
        {
            switch (propertyToReplace)
            {
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

    private async Task<object> HandleCategoryUpdateAsync(ImportUpdatePunchItemCommand request, PunchItem punchItem, CancellationToken cancellationToken)
    {
        if (request.Category is null)
        {
            return ToImportError(request, "Missing Category, cannot update");
        }

        var category = request.Category.Value;
        
        var change = new ChangedProperty<string>(
            nameof(PunchItem.Category),
            punchItem.Category.ToString(),
            category.ToString());

        punchItem.Category = category;
        
        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            $"Punch item category changed to {request.Category}",
            [change],
            cancellationToken);
        
        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated as {PunchItemCategory}",
            punchItem.ItemNo,
            punchItem.Guid,
            punchItem.Category);

        return integrationEvent;
    }

    private async Task<object> HandleRejectAsync(ImportUpdatePunchItemCommand request, PunchItem punchItem,
        CancellationToken cancellationToken)
    {
        if (request is { RejectedBy: { HasValue: false } } or { RejectedBy: { HasValue: true, Value: null } })
        {
            return ToImportError(request, "Missing RejectedBy, cannot clear");
        }

        var currentPerson =
            await personRepository.GetAsync(request.RejectedBy.Value.Value.PersonOid, cancellationToken);
        var change = new ChangedProperty<string?>(RejectReasonPropertyName, null, "IMPORTED REJECTION");

        punchItem.Reject(currentPerson);

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
        if (request is { VerifiedBy: { HasValue: false } } or { VerifiedBy: { HasValue: true, Value: null } })
        {
            return ToImportError(request, "Missing VerifiedBy, cannot clear");
        }

        var currentPerson =
            await personRepository.GetAsync(request.VerifiedBy.Value.Value.PersonOid, cancellationToken);

        punchItem.Verify(currentPerson);

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
        if (request is { ClearedBy: { HasValue: false } } or { ClearedBy: { HasValue: true, Value: null } })
        {
            return ToImportError(request, "Missing CleardBy, cannot clear");
        }

        var currentPerson = await personRepository.GetAsync(request.ClearedBy.Value.Value.PersonOid, cancellationToken);

        punchItem.Clear(currentPerson);

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
