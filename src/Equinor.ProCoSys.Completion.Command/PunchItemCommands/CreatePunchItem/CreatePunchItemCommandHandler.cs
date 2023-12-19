using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
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
    private readonly IProjectRepository _projectRepository;
    private readonly IPersonCache _personCache;
    private readonly IPersonRepository _personRepository;
    private readonly IWorkOrderRepository _woRepository;
    private readonly ISWCRRepository _swcrRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePunchItemCommandHandler(
        IPlantProvider plantProvider,
        IPunchItemRepository punchItemRepository,
        ILibraryItemRepository libraryItemRepository,
        IProjectRepository projectRepository,
        IPersonRepository personRepository,
        IPersonCache personCache,
        IWorkOrderRepository woRepository,
        ISWCRRepository swcrRepository,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreatePunchItemCommandHandler> logger)
    {
        _plantProvider = plantProvider;
        _punchItemRepository = punchItemRepository;
        _libraryItemRepository = libraryItemRepository;
        _projectRepository = projectRepository;
        _logger = logger;
        _personRepository = personRepository;
        _personCache = personCache;
        _woRepository = woRepository;
        _swcrRepository = swcrRepository;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetAsync(request.ProjectGuid, cancellationToken);

        var raisedByOrg = await _libraryItemRepository.GetByGuidAndTypeAsync(
            request.RaisedByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);
        var clearingByOrg = await _libraryItemRepository.GetByGuidAndTypeAsync(
            request.ClearingByOrgGuid,
            LibraryType.COMPLETION_ORGANIZATION,
            cancellationToken);

        var punchItem = new PunchItem(
            _plantProvider.Plant,
            project,
            request.CheckListGuid,
            request.Category,
            request.Description,
            raisedByOrg,
            clearingByOrg);

        await SetActionByAsync(punchItem, request.ActionByPersonOid, cancellationToken);
        punchItem.DueTimeUtc = request.DueTimeUtc;
        await SetLibraryItemAsync(punchItem, request.PriorityGuid, LibraryType.PUNCHLIST_PRIORITY, cancellationToken);
        await SetLibraryItemAsync(punchItem, request.SortingGuid, LibraryType.PUNCHLIST_SORTING, cancellationToken);
        await SetLibraryItemAsync(punchItem, request.TypeGuid, LibraryType.PUNCHLIST_TYPE, cancellationToken);
        punchItem.Estimate = request.Estimate;
        await SetOriginalWorkOrderAsync(punchItem, request.OriginalWorkOrderGuid, cancellationToken);
        await SetWorkOrderAsync(punchItem, request.WorkOrderGuid, cancellationToken);
        await SetSWCRAsync(punchItem, request.SWCRGuid, cancellationToken);
        await SetDocumentAsync(punchItem, request.DocumentGuid, cancellationToken);
        punchItem.ExternalItemNo = request.ExternalItemNo;
        punchItem.MaterialRequired = request.MaterialRequired;
        punchItem.MaterialETAUtc = request.MaterialETAUtc;
        punchItem.MaterialExternalNo = request.MaterialExternalNo;

        _punchItemRepository.Add(punchItem);
        punchItem.AddDomainEvent(new PunchItemCreatedDomainEvent(punchItem));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} created", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(punchItem.Guid, punchItem.RowVersion.ConvertToString()));
    }

    private async Task SetDocumentAsync(PunchItem punchItem, Guid? documentGuid, CancellationToken cancellationToken)
    {
        if (!documentGuid.HasValue)
        {
            return;
        }

        var doc = await _documentRepository.GetAsync(documentGuid.Value, cancellationToken);
        punchItem.SetDocument(doc);
    }

    private async Task SetSWCRAsync(PunchItem punchItem, Guid? swcrGuid, CancellationToken cancellationToken)
    {
        if (!swcrGuid.HasValue)
        {
            return;
        }

        var swcr = await _swcrRepository.GetAsync(swcrGuid.Value, cancellationToken);
        punchItem.SetSWCR(swcr);
    }

    private async Task SetOriginalWorkOrderAsync(PunchItem punchItem, Guid? originalWorkOrderGuid, CancellationToken cancellationToken)
    {
        if (!originalWorkOrderGuid.HasValue)
        {
            return;
        }

        var wo = await _woRepository.GetAsync(originalWorkOrderGuid.Value, cancellationToken);
        punchItem.SetOriginalWorkOrder(wo);
    }

    private async Task SetWorkOrderAsync(PunchItem punchItem, Guid? workOrderGuid, CancellationToken cancellationToken)
    {
        if (!workOrderGuid.HasValue)
        {
            return;
        }

        var wo = await _woRepository.GetAsync(workOrderGuid.Value, cancellationToken);
        punchItem.SetWorkOrder(wo);
    }

    private async Task SetActionByAsync(PunchItem punchItem, Guid? actionByPersonOid, CancellationToken cancellationToken)
    {
        if (!actionByPersonOid.HasValue)
        {
            return;
        }

        var person = await GetOrCreatePersonAsync(actionByPersonOid.Value, cancellationToken);
        punchItem.SetActionBy(person);
    }

    private async Task<Person> GetOrCreatePersonAsync(Guid oid, CancellationToken cancellationToken)
    {
        var personExists = await _personRepository.ExistsAsync(oid, cancellationToken);
        if (personExists)
        {
            return await _personRepository.GetAsync(oid, cancellationToken);
        }

        var pcsPerson = await _personCache.GetAsync(oid);
        var person = new Person(oid, pcsPerson.FirstName, pcsPerson.LastName, pcsPerson.UserName, pcsPerson.Email);
        _personRepository.Add(person);

        return person;
    }

    private async Task SetLibraryItemAsync(
        PunchItem punchItem,
        Guid? libraryGuid,
        LibraryType libraryType,
        CancellationToken cancellationToken)
    {
        if (!libraryGuid.HasValue)
        {
            return;
        }
        var libraryItem = await _libraryItemRepository.GetByGuidAndTypeAsync(libraryGuid.Value, libraryType, cancellationToken);

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
}
