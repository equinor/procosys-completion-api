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

        var changes = await PatchAsync(punchItem, request);

        punchItem.SetRowVersion(request.RowVersion);

        if (changes.Any())
        {
            punchItem.AddDomainEvent(new PunchItemUpdatedDomainEvent(punchItem, changes));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }

    private async Task<List<IProperty>> PatchAsync(PunchItem punchItem, UpdatePunchItemCommand request)
    {
        var changes = new List<IProperty>();
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
            IProperty? change;
            switch (prop)
            {
                case nameof(PatchablePunchItem.Description):
                    if (punchItem.Description != patchedPunchItem.Description)
                    {
                        change = new Property<string>(nameof(punchItem.Description),
                            punchItem.Description,
                            patchedPunchItem.Description);
                        changes.AddChangeIfNotNull(change);
                        punchItem.Description = patchedPunchItem.Description;
                    }
                    break;

                case nameof(PatchablePunchItem.RaisedByOrgGuid):
                    change = await SetLibraryItemAsync(prop, punchItem, patchedPunchItem.RaisedByOrgGuid,
                        LibraryType.COMPLETION_ORGANIZATION);
                    changes.AddChangeIfNotNull(change);
                    break;

                case nameof(PatchablePunchItem.ClearingByOrgGuid):
                    change = await SetLibraryItemAsync(prop, punchItem, patchedPunchItem.ClearingByOrgGuid,
                        LibraryType.COMPLETION_ORGANIZATION);
                    changes.AddChangeIfNotNull(change);
                    break;

                case nameof(PatchablePunchItem.PriorityGuid):
                    change = await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.PriorityGuid,
                        LibraryType.PUNCHLIST_PRIORITY);
                    changes.AddChangeIfNotNull(change);
                    break;

                case nameof(PatchablePunchItem.SortingGuid):
                    change = await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.SortingGuid,
                        LibraryType.PUNCHLIST_SORTING);
                    changes.AddChangeIfNotNull(change);
                    break;

                case nameof(PatchablePunchItem.TypeGuid):
                    change = await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.TypeGuid,
                        LibraryType.PUNCHLIST_TYPE);
                    changes.AddChangeIfNotNull(change);
                    break;

                default:
                    throw new NotImplementedException($"Patching property {prop} not implemented");
            }
        }

        return changes;
    }

    private async Task<IProperty?> SetOrClearLibraryItemAsync(
        string property,
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType)
    {
        if (libraryGuid.HasValue)
        {
            return await SetLibraryItemAsync(property, punchItem, libraryGuid.Value, libraryType);
        }

        return ClearLibraryItemAsync(property, punchItem, libraryType);
    }

    private async Task<IProperty?> SetLibraryItemAsync(
        string property,
        PunchItem punchItem,
        Guid libraryGuid,
        LibraryType libraryType)
    {
        IProperty? change = null;
        switch (property)
        {
            case nameof(PatchablePunchItem.RaisedByOrgGuid):
                if (punchItem.RaisedByOrg.Guid != libraryGuid)
                {
                    var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid, libraryType);
                    change = new Property<string>(nameof(punchItem.RaisedByOrg),
                        punchItem.RaisedByOrg.Code,
                        libraryItem.Code);
                    punchItem.SetRaisedByOrg(libraryItem);
                }
                break;
            case nameof(PatchablePunchItem.ClearingByOrgGuid):
                if (punchItem.ClearingByOrg.Guid != libraryGuid)
                {
                    var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid, libraryType);
                    change = new Property<string>(nameof(punchItem.ClearingByOrg),
                        punchItem.ClearingByOrg.Code,
                        libraryItem.Code);
                    punchItem.SetClearingByOrg(libraryItem);
                }
                break;
            case nameof(PatchablePunchItem.PriorityGuid):
                if (punchItem.Priority is null || punchItem.Priority.Guid != libraryGuid)
                {
                    var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid, libraryType);
                    change = new Property<string>(nameof(punchItem.Priority),
                        punchItem.Priority?.Code,
                        libraryItem.Code);
                    punchItem.SetPriority(libraryItem);
                }
                break;
            case nameof(PatchablePunchItem.SortingGuid):
                if (punchItem.Sorting is null || punchItem.Sorting.Guid != libraryGuid)
                {
                    var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid, libraryType);
                    change = new Property<string>(nameof(punchItem.Sorting),
                        punchItem.Sorting?.Code,
                        libraryItem.Code);
                    punchItem.SetSorting(libraryItem);
                }
                break;
            case nameof(PatchablePunchItem.TypeGuid):
                if (punchItem.Type is null || punchItem.Type.Guid != libraryGuid)
                {
                    var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid, libraryType);
                    change = new Property<string>(nameof(punchItem.Type),
                        punchItem.Type?.Code,
                        libraryItem.Code);
                    punchItem.SetType(libraryItem);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }

        return change;
    }

    private IProperty? ClearLibraryItemAsync(
        string property,
        PunchItem punchItem,
        LibraryType libraryType)
    {
        IProperty? change = null;
        switch (property)
        {
            case nameof(PatchablePunchItem.PriorityGuid):
                if (punchItem.Priority is not null)
                {
                    change = new Property<string?>(nameof(punchItem.Priority),
                        punchItem.Priority.Code,
                        null);
                    punchItem.ClearPriority();
                }
                break;
            case nameof(PatchablePunchItem.SortingGuid):
                if (punchItem.Sorting is not null)
                {
                    change = new Property<string?>(nameof(punchItem.Sorting),
                        punchItem.Sorting.Code,
                        null);
                    punchItem.ClearSorting();
                }
                break;
            case nameof(PatchablePunchItem.TypeGuid):
                if (punchItem.Type is not null)
                {
                    change = new Property<string?>(nameof(punchItem.Type),
                        punchItem.Type.Code,
                        null);
                    punchItem.ClearType();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }

        return change;
    }
}
