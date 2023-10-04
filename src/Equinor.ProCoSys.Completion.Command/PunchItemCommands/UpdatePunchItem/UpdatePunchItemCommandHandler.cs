using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommandHandler : IRequestHandler<UpdatePunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ILibraryItemRepository _libraryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePunchItemCommandHandler> _logger;

    public UpdatePunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ILibraryItemRepository libraryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _libraryItemRepository = libraryItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UpdatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemRepository.GetByGuidAsync(request.PunchItemGuid);
        if (punchItem is null)
        {
            throw new Exception($"Entity {nameof(PunchItem)} {request.PunchItemGuid} not found");
        }

        var changes = await PatchAsync(punchItem, request.PatchDocument);

        if (changes.Any())
        {
            punchItem.AddDomainEvent(new PunchItemUpdatedDomainEvent(punchItem, changes));
        }

        punchItem.SetRowVersion(request.RowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }

    private async Task<List<IProperty>> PatchAsync(
        PunchItem punchItem,
        JsonPatchDocument<PatchablePunchItem> patchDocument)
    {
        var changes = new List<IProperty>();

        var propertiesToReplace = GetPropertiesToReplace(patchDocument);
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
                    if (punchItem.Description != patchedPunchItem.Description)
                    {
                        changes.Add(new Property<string>(nameof(punchItem.Description),
                            punchItem.Description,
                            patchedPunchItem.Description));
                        punchItem.Description = patchedPunchItem.Description;
                    }
                    break;

                case nameof(PatchablePunchItem.RaisedByOrgGuid):
                    if (punchItem.RaisedByOrg.Guid != patchedPunchItem.RaisedByOrgGuid)
                    {
                        var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
                            patchedPunchItem.RaisedByOrgGuid,
                            LibraryType.COMPLETION_ORGANIZATION);
                        changes.Add(new Property<string>(nameof(punchItem.RaisedByOrg),
                            punchItem.RaisedByOrg.Code,
                            libraryItem.Code));
                        punchItem.SetRaisedByOrg(libraryItem);
                    }
                    break;

                case nameof(PatchablePunchItem.ClearingByOrgGuid):
                    if (punchItem.ClearingByOrg.Guid != patchedPunchItem.ClearingByOrgGuid)
                    {
                        var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
                            patchedPunchItem.ClearingByOrgGuid, 
                            LibraryType.COMPLETION_ORGANIZATION);
                        changes.Add(new Property<string>(nameof(punchItem.ClearingByOrg),
                            punchItem.ClearingByOrg.Code,
                            libraryItem.Code));
                        punchItem.SetClearingByOrg(libraryItem);
                    }
                    break;

                case nameof(PatchablePunchItem.PriorityGuid):
                    if (punchItem.Priority?.Guid != patchedPunchItem.PriorityGuid)
                    {
                        if (patchedPunchItem.PriorityGuid is not null)
                        {
                            var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
                                patchedPunchItem.PriorityGuid.Value,
                                LibraryType.PUNCHLIST_PRIORITY);
                            changes.Add(new Property<string?>(nameof(punchItem.Priority),
                                punchItem.Priority?.Code,
                                libraryItem.Code));
                            punchItem.SetPriority(libraryItem);
                        }
                        else
                        {
                            changes.Add(new Property<string?>(nameof(punchItem.Priority),
                                punchItem.Priority!.Code,
                                null));
                            punchItem.ClearPriority();
                        }
                    }
                    break;

                case nameof(PatchablePunchItem.SortingGuid):
                    if (punchItem.Sorting?.Guid != patchedPunchItem.SortingGuid)
                    {
                        if (patchedPunchItem.SortingGuid is not null)
                        {
                            var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
                                patchedPunchItem.SortingGuid.Value,
                                LibraryType.PUNCHLIST_SORTING);
                            changes.Add(new Property<string?>(nameof(punchItem.Sorting),
                                punchItem.Sorting?.Code,
                                libraryItem.Code));
                            punchItem.SetSorting(libraryItem);
                        }
                        else
                        {
                            changes.Add(new Property<string?>(nameof(punchItem.Sorting),
                                punchItem.Sorting!.Code,
                                null));
                            punchItem.ClearSorting();
                        }
                    }
                    break;

                case nameof(PatchablePunchItem.TypeGuid):
                    if (punchItem.Type?.Guid != patchedPunchItem.TypeGuid)
                    {
                        if (patchedPunchItem.TypeGuid is not null)
                        {
                            var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(
                                patchedPunchItem.TypeGuid.Value,
                                LibraryType.PUNCHLIST_TYPE);
                            changes.Add(new Property<string?>(nameof(punchItem.Type),
                                punchItem.Type?.Code,
                                libraryItem.Code));
                            punchItem.SetType(libraryItem);
                        }
                        else
                        {
                            changes.Add(new Property<string?>(nameof(punchItem.Type),
                                punchItem.Type!.Code,
                                null));
                            punchItem.ClearType();
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException($"Patching property {propertyToReplace} not implemented");
            }
        }

        return changes;
    }

    private static List<string> GetPropertiesToReplace(JsonPatchDocument<PatchablePunchItem> patchDocument)
        => patchDocument.Operations
            .Where(op => op.OperationType == OperationType.Replace)
            .Select(op => op.path.TrimStart('/')).ToList();
}
