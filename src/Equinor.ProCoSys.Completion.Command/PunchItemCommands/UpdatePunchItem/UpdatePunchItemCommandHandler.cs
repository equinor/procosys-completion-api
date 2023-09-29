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
using MediatR;
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

        var changes = await Patch(punchItem, request);

        punchItem.SetRowVersion(request.RowVersion);

        if (changes.Any())
        {
            punchItem.AddDomainEvent(new PunchItemUpdatedDomainEvent(punchItem, changes));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }

    private async Task<List<Property>> Patch(PunchItem punchItem, UpdatePunchItemCommand request)
    {
        var changes = new List<Property>();
        var operations = request.PatchDocument.Operations;

        var replaceProperties = operations.Select(op => op.path.TrimStart('/')).ToList();
        if (!replaceProperties.Any())
        {
            return changes;
        }

        var patchedPunchItem = new PatchablePunchItem();

        request.PatchDocument.ApplyTo(patchedPunchItem);

        foreach (var prop in replaceProperties)
        {
            switch (prop)
            {
                case nameof(PatchablePunchItem.Description):
                    if (punchItem.Description != patchedPunchItem.Description)
                    {
                        changes.Add(new Property(nameof(punchItem.Description),
                            punchItem.Description,
                            patchedPunchItem.Description));
                        punchItem.Description = patchedPunchItem.Description;
                    }

                    break;

                case nameof(PatchablePunchItem.RaisedByOrgGuid):
                    await SetLibraryItemAsync(prop, punchItem, patchedPunchItem.RaisedByOrgGuid,
                        LibraryType.COMPLETION_ORGANIZATION, changes);
                    break;

                case nameof(PatchablePunchItem.ClearingByOrgGuid):
                    await SetLibraryItemAsync(prop, punchItem, patchedPunchItem.ClearingByOrgGuid,
                        LibraryType.COMPLETION_ORGANIZATION, changes);
                    break;

                case nameof(PatchablePunchItem.PriorityGuid):
                    await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.PriorityGuid,
                        LibraryType.PUNCHLIST_PRIORITY, changes);
                    break;

                case nameof(PatchablePunchItem.SortingGuid):
                    await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.SortingGuid,
                        LibraryType.PUNCHLIST_SORTING, changes);
                    break;

                case nameof(PatchablePunchItem.TypeGuid):
                    await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.TypeGuid,
                        LibraryType.PUNCHLIST_TYPE, changes);
                    break;

                default:
                    throw new NotImplementedException($"Patching property {prop} not implemented");
            }
        }

        return changes;
    }

    private async Task SetOrClearLibraryItemAsync(
        string property,
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType,
        List<Property> changes)
    {
        if (libraryGuid.HasValue)
        {
            await SetLibraryItemAsync(property, punchItem, libraryGuid.Value, libraryType, changes);
        }
        else
        {
            ClearLibraryItemAsync(property, punchItem, libraryType, changes);
        }
    }

    private async Task SetLibraryItemAsync(
        string property,
        PunchItem punchItem,
        Guid libraryGuid,
        LibraryType libraryType,
        List<Property> changes)
    {
        var libraryItem = (await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid, libraryType))!;

        switch (property)
        {
            case nameof(PatchablePunchItem.RaisedByOrgGuid):
                if (punchItem.RaisedByOrg.Guid != libraryGuid)
                {
                    changes.Add(new Property(nameof(punchItem.RaisedByOrg),
                        punchItem.RaisedByOrg.Code,
                        libraryItem.Code));
                    punchItem.SetRaisedByOrg(libraryItem);
                }
                break;
            case nameof(PatchablePunchItem.ClearingByOrgGuid):
                if (punchItem.ClearingByOrg.Guid != libraryGuid)
                {
                    changes.Add(new Property(nameof(punchItem.ClearingByOrg),
                        punchItem.ClearingByOrg.Code,
                        libraryItem.Code));
                    punchItem.SetClearingByOrg(libraryItem);
                }
                break;
            case nameof(PatchablePunchItem.PriorityGuid):
                if (punchItem.Priority is null || punchItem.Priority.Guid != libraryGuid)
                {
                    changes.Add(new Property(nameof(punchItem.Priority),
                        punchItem.Priority?.Code,
                        libraryItem.Code));
                    punchItem.SetPriority(libraryItem);
                }
                break;
            case nameof(PatchablePunchItem.SortingGuid):
                if (punchItem.Sorting is null || punchItem.Sorting.Guid != libraryGuid)
                {
                    changes.Add(new Property(nameof(punchItem.Sorting),
                        punchItem.Sorting?.Code,
                        libraryItem.Code));
                    punchItem.SetSorting(libraryItem);
                }
                break;
            case nameof(PatchablePunchItem.TypeGuid):
                if (punchItem.Type is null || punchItem.Type.Guid != libraryGuid)
                {
                    changes.Add(new Property(nameof(punchItem.Type),
                        punchItem.Type?.Code,
                        libraryItem.Code));
                    punchItem.SetType(libraryItem);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }
    }

    private void ClearLibraryItemAsync(
        string property,
        PunchItem punchItem,
        LibraryType libraryType,
        List<Property> changes)
    {
        switch (property)
        {
            case nameof(PatchablePunchItem.PriorityGuid):
                if (punchItem.Priority is not null)
                {
                    changes.Add(new Property(nameof(punchItem.Priority),
                        punchItem.Priority.Code,
                        null));
                    punchItem.ClearPriority();
                }
                break;
            case nameof(PatchablePunchItem.SortingGuid):
                if (punchItem.Sorting is not null)
                {
                    changes.Add(new Property(nameof(punchItem.Sorting),
                        punchItem.Sorting.Code,
                        null));
                    punchItem.ClearSorting();
                }
                break;
            case nameof(PatchablePunchItem.TypeGuid):
                if (punchItem.Type is not null)
                {
                    changes.Add(new Property(nameof(punchItem.Type),
                        punchItem.Type.Code,
                        null));
                    punchItem.ClearType();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }
    }
}
