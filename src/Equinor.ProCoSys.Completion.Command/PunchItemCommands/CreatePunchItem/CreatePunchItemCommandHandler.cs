using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using ServiceResult;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommandHandler : IRequestHandler<CreatePunchItemCommand, Result<GuidAndRowVersion>>
{
    private readonly ILogger<CreatePunchItemCommandHandler> _logger;

    private readonly IPlantProvider _plantProvider;
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectRepository _projectRepository;

    public CreatePunchItemCommandHandler(
        IPlantProvider plantProvider,
        IPunchItemRepository punchItemRepository,
        IUnitOfWork unitOfWork,
        IProjectRepository projectRepository,
        ILogger<CreatePunchItemCommandHandler> logger)
    {
        _plantProvider = plantProvider;
        _punchItemRepository = punchItemRepository;
        _unitOfWork = unitOfWork;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByGuidAsync(request.ProjectGuid);
        if (project is null)
        {
            throw new Exception($"Could not find ProCoSys project with Guid {request.ProjectGuid} in plant {_plantProvider.Plant}");
        }

        var punchItem = new PunchItem(_plantProvider.Plant, project, request.ItemNo);
        _punchItemRepository.Add(punchItem);
        punchItem.AddDomainEvent(new PunchItemCreatedDomainEvent(punchItem, request.ProjectGuid));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} created", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(punchItem.Guid, punchItem.RowVersion.ConvertToString()));
    }
}
