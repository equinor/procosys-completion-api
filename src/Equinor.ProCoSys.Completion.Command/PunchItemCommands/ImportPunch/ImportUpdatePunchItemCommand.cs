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
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;

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
    string RowVersion) : ICanHaveRestrictionsViaCheckList, IRequest<List<ImportError>>, IIsPunchItemCommand , IImportCommand
{
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
}

public sealed class ImportUpdatePunchItemHandler(
    ILabelValidator labelValidator,
    IOptionsMonitor<ApplicationOptions> options,
    ILibraryItemValidator libraryItemValidator,
    IWorkOrderValidator workOrderValidator,
    ISWCRValidator swcrValidator,
    IDocumentValidator documentValidator,
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
            var command = new ClearPunchItemCommand(
                        request.PunchItemGuid,
                        request.RowVersion);
            command.PunchItem = request.PunchItem;
            command.CheckListDetailsDto = request.CheckListDetailsDto;

            var results =
                await clearValidator.ValidateAsync(
                    command,
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (IsVerifyingPunch(request))
        {
            var command = new VerifyPunchItemCommand(
                    request.PunchItemGuid,
                    request.RowVersion)
            {
                PunchItem = request.PunchItem,
                CheckListDetailsDto = request.CheckListDetailsDto
            };

            var results =
                await verifyValidator.ValidateAsync(
                    command,
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (IsRejectingPunch(request))
        {
            var command = new RejectPunchItemCommand(
                    request.PunchItemGuid,
                    string.Empty,
                    [],
                    request.RowVersion)
            {
                PunchItem = request.PunchItem,
                CheckListDetailsDto = request.CheckListDetailsDto
            };

            var results =
                await rejectValidator.ValidateAsync(
                    command,
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (IsUpdatingPunchCategory(request))
        {
            var command = new UpdatePunchItemCategoryCommand(
                        request.PunchItemGuid,
                        request.Category!.Value,
                        request.RowVersion)
            {
                PunchItem = request.PunchItem,
                CheckListDetailsDto = request.CheckListDetailsDto
            };

            var results =
                await categoryValidator.ValidateAsync(
                    command,
                    cancellationToken);
            errors.AddRange(results.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        if (IsGeneralUpdate(request))
        {
            var command = new UpdatePunchItemCommand(
            request.PunchItemGuid,
            request.PatchDocument,
            request.RowVersion)
            {
                PunchItem = request.PunchItem,
                CheckListDetailsDto = request.CheckListDetailsDto
            };

            var updateResults =
                await updateValidator.ValidateAsync(
                    command,
                    cancellationToken);
            errors.AddRange(updateResults.Errors.Select(x => ToImportError(request, x.ErrorMessage)));
        }

        return [.. errors];
    }

    private static bool IsGeneralUpdate(ImportUpdatePunchItemCommand request) => request.PatchDocument.Operations.Count != 0;

    private static bool IsUpdatingPunchCategory(ImportUpdatePunchItemCommand request) => request.Category.HasValue;

    private static bool IsRejectingPunch(ImportUpdatePunchItemCommand request) => request.RejectedBy.HasValue;

    private static bool IsVerifyingPunch(ImportUpdatePunchItemCommand request) => request.VerifiedBy.HasValue;

    private static bool IsClearingPunch(ImportUpdatePunchItemCommand request) => request.ClearedBy.HasValue;

    public async Task<List<ImportError>> Handle(ImportUpdatePunchItemCommand request,
        CancellationToken cancellationToken)
    {
        var errors = (await Validate(request, cancellationToken)).ToList();
        if (errors.Count != 0)
        {
            return errors;
        }

        var events = new List<object>();
        var punchItem = request.PunchItem;

        await unitOfWork.SetAuditDataAsync();

        errors.AddRange(await PatchPunchAsync(request, punchItem, events, cancellationToken));

        await unitOfWork.SetAuditDataAsync();
        punchItem.SetRowVersion(request.RowVersion);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong when commiting import for Punch with Import Guid '{Guid}'",
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
            logger.LogError(e, "Error occurred while trying to Sync Clear on PunchItemList with guid {PunchItemGuid}",
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
                "Error occurred while trying to Recalculate the CheckListStatus for CheckList with Guid {Guid}",
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
        
        if (IsUpdatingPunchCategory(request))
        {
            var updateCategoryEvent = await HandleCategoryUpdateAsync(request, punchItem, cancellationToken);
            if (updateCategoryEvent is ImportError e)
            {
                errors = [..errors, e];
            }
            else
            {
                events.Add(updateCategoryEvent);
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

 //               case nameof(PatchablePunchItem.ExternalItemNo):
 //                   PunchItemPatcher.PatchExternalItemNo(punchItem, patchedPunchItem, changes);
 //                   break;

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

        await unitOfWork.SetAuditDataAsync();

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
            return ToImportError(request, "Missing RejectedBy, cannot reject");
        }

        var currentPerson =
            await personRepository.GetOrCreateAsync(request.RejectedBy.Value!.PersonOid, cancellationToken);
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
            return ToImportError(request, "Missing VerifiedBy, cannot verify");
        }

        var currentPerson =
            await personRepository.GetOrCreateAsync(request.VerifiedBy.Value!.PersonOid, cancellationToken);

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
            return ToImportError(request, "Missing ClearedBy, cannot clear");
        }

        var currentPerson = await personRepository.GetOrCreateAsync(request.ClearedBy.Value!.PersonOid, cancellationToken);
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
