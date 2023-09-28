using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommandHandler : IRequestHandler<CreatePunchItemCommand, Result<GuidAndRowVersion>>
{
    private readonly ILogger<CreatePunchItemCommandHandler> _logger;

    private readonly IPlantProvider _plantProvider;
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ILibraryItemRepository _libraryItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectRepository _projectRepository;

    public CreatePunchItemCommandHandler(
        IPlantProvider plantProvider,
        IPunchItemRepository punchItemRepository,
        ILibraryItemRepository libraryItemRepository,
        IUnitOfWork unitOfWork,
        IProjectRepository projectRepository,
        ILogger<CreatePunchItemCommandHandler> logger)
    {
        _plantProvider = plantProvider;
        _punchItemRepository = punchItemRepository;
        _libraryItemRepository = libraryItemRepository;
        _unitOfWork = unitOfWork;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByGuidAsync(request.ProjectGuid);
        if (project is null)
        {
            throw new Exception($"Could not find {nameof(Project)} with Guid {request.ProjectGuid} in plant {_plantProvider.Plant}");
        }

        var raisedByOrg = await GetLibraryItemAsync(request.RaisedByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);
        var clearingByOrg = await GetLibraryItemAsync(request.ClearingByOrgGuid, LibraryType.COMPLETION_ORGANIZATION);

        var punchItem = new PunchItem(
            _plantProvider.Plant,
            project,
            request.CheckListGuid,
            request.Category,
            request.Description,
            raisedByOrg,
            clearingByOrg);

        await SetLibraryItemAsync(punchItem, request.PriorityGuid, LibraryType.PUNCHLIST_PRIORITY);
        await SetLibraryItemAsync(punchItem, request.SortingGuid, LibraryType.PUNCHLIST_SORTING);
        await SetLibraryItemAsync(punchItem, request.TypeGuid, LibraryType.PUNCHLIST_TYPE);

        _punchItemRepository.Add(punchItem);
        punchItem.AddDomainEvent(new PunchItemCreatedDomainEvent(punchItem));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} created", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(punchItem.Guid, punchItem.RowVersion.ConvertToString()));
    }

    private async Task SetLibraryItemAsync(PunchItem punchItem, Guid? libraryGuid, LibraryType libraryType)
    {
        if (!libraryGuid.HasValue)
        {
            return;
        }
        var libraryItem = await GetLibraryItemAsync(libraryGuid.Value, libraryType);

        switch (libraryType)
        {
            case LibraryType.PUNCHLIST_PRIORITY:
                punchItem.SetPriority(libraryItem);
                break;
            case LibraryType.PUNCHLIST_SORTING:
                punchItem.SetSorting(libraryItem);
                break;
            case LibraryType.PUNCHLIST_TYPE:
                punchItem.SetType(libraryItem);
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
