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
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;

public class CreatePunchCommandHandler : IRequestHandler<CreatePunchCommand, Result<GuidAndRowVersion>>
{
    private readonly ILogger<CreatePunchCommandHandler> _logger;

    private readonly IPlantProvider _plantProvider;
    private readonly IPunchRepository _punchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectRepository _projectRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePunchCommandHandler(
        IPlantProvider plantProvider,
        IPunchRepository punchRepository,
        IUnitOfWork unitOfWork,
        IProjectRepository projectRepository,
        ILogger<CreatePunchCommandHandler> logger, 
        IPublishEndpoint publishEndpoint)
    {
        _plantProvider = plantProvider;
        _punchRepository = punchRepository;
        _unitOfWork = unitOfWork;
        _projectRepository = projectRepository;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByGuidAsync(request.ProjectGuid);
        if (project is null)
        {
            throw new Exception($"Could not find ProCoSys project with Guid {request.ProjectGuid} in plant {_plantProvider.Plant}");
        }

        var punch = new Punch(_plantProvider.Plant, project, request.ItemNo);
        _punchRepository.Add(punch);
        
        //await PublishDistributed(new PunchCreated(punch,request.ProjectGuid), cancellationToken);
        
        punch.AddDomainEvent(new PunchCreatedEvent(punch, request.ProjectGuid));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch '{PunchItemNo}' with guid {PunchGuid} created", punch.ItemNo, punch.Guid);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(punch.Guid, punch.RowVersion.ConvertToString()));
    }

    private async Task PublishDistributed(PunchCreatedIntegrationEvent punchCreatedIntegrationEvent, CancellationToken cancellationToken)
    {
        var sessionId = punchCreatedIntegrationEvent.Guid.ToString();
        await _publishEndpoint.Publish(punchCreatedIntegrationEvent,
            context =>
            {
                context.SetSessionId(sessionId);
                _logger.LogInformation("Published: {Message}", context.Message.ToString());
            },
            cancellationToken);
    }
}
