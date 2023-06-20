using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using ServiceResult;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;

public class CreatePunchCommandHandler : IRequestHandler<CreatePunchCommand, Result<GuidAndRowVersion>>
{
    private readonly ILogger<CreatePunchCommandHandler> _logger;

    private readonly IPlantProvider _plantProvider;
    private readonly IPunchRepository _punchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectRepository _projectRepository;

    public CreatePunchCommandHandler(
        IPlantProvider plantProvider,
        IPunchRepository punchRepository,
        IUnitOfWork unitOfWork,
        IProjectRepository projectRepository,
        ILogger<CreatePunchCommandHandler> logger)
    {
        _plantProvider = plantProvider;
        _punchRepository = punchRepository;
        _unitOfWork = unitOfWork;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.TryGetByGuidAsync(request.ProjectGuid);
        if (project is null)
        {
            throw new Exception($"Could not find ProCoSys project with Guid {request.ProjectGuid} in plant {_plantProvider.Plant}");
        }

        var punch = new Punch(_plantProvider.Plant, project, request.Title);
        _punchRepository.Add(punch);
        punch.AddDomainEvent(new PunchCreatedEvent(punch, request.ProjectGuid));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Punch '{request.Title}' created");

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(punch.Guid, punch.RowVersion.ConvertToString()));
    }
}
