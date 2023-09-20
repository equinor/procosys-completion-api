using System;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MediatR;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommandHandler : IRequestHandler<UpdatePunchItemCommand, Result<string>>
{
    private readonly IPlantProvider _plantProvider;
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ILibraryItemRepository _libraryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePunchItemCommandHandler> _logger;

    public UpdatePunchItemCommandHandler(
        IPlantProvider plantProvider,
        IPunchItemRepository punchItemRepository,
        ILibraryItemRepository libraryItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePunchItemCommandHandler> logger)
    {
        _plantProvider = plantProvider;
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

        var patches = await Patch(punchItem, request);
        if (patches is null)
        {
            _logger.LogInformation("Early exit. No changes in punch item '{PunchItemNo}' with guid {PunchItemGuid}", punchItem.ItemNo, punchItem.Guid);
            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }

        punchItem.SetRowVersion(request.RowVersion);

        // todo 104046 Refactor PunchItemUpdatedDomainEvent to take a dynamic object with props actually patched
        punchItem.AddDomainEvent(new PunchItemUpdatedDomainEvent(punchItem));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }

    private async Task<ExpandoObject?> Patch(PunchItem punchItem, UpdatePunchItemCommand request)
    {
        var operations = request.PatchDocument.Operations;
        if (operations.Any(op => op.OperationType != OperationType.Replace))
        {
            throw new Exception($"Only {OperationType.Replace} supported");
        }

        var replaceProperties = operations.Select(op => op.path.TrimStart('/')).ToList();
        if (!replaceProperties.Any())
        {
            return null;
        }

        var patchedPunchItem = new PatchablePunchItem();
        dynamic patch = new ExpandoObject();

        request.PatchDocument.ApplyTo(patchedPunchItem);

        foreach (var prop in replaceProperties)
        {
            switch (prop)
            {
                case nameof(PatchablePunchItem.Description):
                    punchItem.Description = patchedPunchItem.Description;
                    patch.Description = patchedPunchItem.Description;
                    break;

                case nameof(PatchablePunchItem.RaisedByOrgGuid):
                    await SetLibraryItemAsync(prop, punchItem, patchedPunchItem.RaisedByOrgGuid,
                        LibraryType.COMPLETION_ORGANIZATION);
                    patch.RaisedByOrgGuid = patchedPunchItem.RaisedByOrgGuid;
                    break;

                case nameof(PatchablePunchItem.ClearingByOrgGuid):
                    await SetLibraryItemAsync(prop, punchItem, patchedPunchItem.ClearingByOrgGuid,
                        LibraryType.COMPLETION_ORGANIZATION);
                    patch.ClearingByOrgGuid = patchedPunchItem.ClearingByOrgGuid;
                    break;

                case nameof(PatchablePunchItem.PriorityGuid):
                    await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.PriorityGuid,
                        LibraryType.PUNCHLIST_PRIORITY);
                    patch.PriorityGuid = patchedPunchItem.PriorityGuid!;
                    break;

                case nameof(PatchablePunchItem.SortingGuid):
                    await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.SortingGuid,
                        LibraryType.PUNCHLIST_SORTING);
                    patch.SortingGuid = patchedPunchItem.SortingGuid!;
                    break;

                case nameof(PatchablePunchItem.TypeGuid):
                    await SetOrClearLibraryItemAsync(prop, punchItem, patchedPunchItem.TypeGuid,
                        LibraryType.PUNCHLIST_TYPE);
                    patch.TypeGuid = patchedPunchItem.TypeGuid!;
                    break;

                default:
                    throw new NotImplementedException($"Patching property {prop} not implemented");
            }
        }

        return patch;
    }

    private async Task SetOrClearLibraryItemAsync(
        string property,
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType)
    {
        if (libraryGuid.HasValue)
        {
            await SetLibraryItemAsync(property, punchItem, libraryGuid.Value, libraryType);
        }
        else
        {
            ClearLibraryItemAsync(property, punchItem, libraryType);
        }
    }

    private async Task SetLibraryItemAsync(
        string property,
        PunchItem punchItem,
        Guid libraryGuid,
        LibraryType libraryType)
    {
        var libraryItem = await GetLibraryItemAsync(libraryGuid, libraryType);

        switch (property)
        {
            case nameof(PatchablePunchItem.RaisedByOrgGuid):
                punchItem.SetRaisedByOrg(libraryItem);
                break;
            case nameof(PatchablePunchItem.ClearingByOrgGuid):
                punchItem.SetClearingByOrg(libraryItem);
                break;
            case nameof(PatchablePunchItem.PriorityGuid):
                punchItem.SetPriority(libraryItem);
                break;
            case nameof(PatchablePunchItem.SortingGuid):
                punchItem.SetSorting(libraryItem);
                break;
            case nameof(PatchablePunchItem.TypeGuid):
                punchItem.SetType(libraryItem);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }
    }

    private void ClearLibraryItemAsync(string property, PunchItem punchItem, LibraryType libraryType)
    {
        switch (property)
        {
            case nameof(PatchablePunchItem.PriorityGuid):
                punchItem.ClearPriority();
                break;
            case nameof(PatchablePunchItem.SortingGuid):
                punchItem.ClearSorting();
                break;
            case nameof(PatchablePunchItem.TypeGuid):
                punchItem.ClearType();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(libraryType), libraryType, null);
        }
    }

    private async Task<LibraryItem> GetLibraryItemAsync(Guid libraryGuid, LibraryType type)
    {
        var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid, type);
        if (libraryItem is null)
        {
            throw new Exception(
                $"Could not find {nameof(LibraryItem)} of type {type} with Guid {libraryGuid} in plant {_plantProvider.Plant}");
        }

        return libraryItem;
    }
}
