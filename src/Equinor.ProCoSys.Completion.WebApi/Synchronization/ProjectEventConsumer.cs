using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class ProjectEventConsumer : IConsumer<ProjectEvent>
{
    private readonly ILogger<ProjectEventConsumer> _logger;
    private readonly IPlantSetter _plantSetter;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptions;
    
    public ProjectEventConsumer(ILogger<ProjectEventConsumer> logger,
        IPlantSetter plantSetter,
        IProjectRepository projectRepository, 
        IUnitOfWork unitOfWork, 
        ICurrentUserSetter currentUserSetter, 
        IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        _logger = logger;
        _plantSetter = plantSetter;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _applicationOptions = applicationOptions;
    }

    public async Task Consume(ConsumeContext<ProjectEvent> context)
    {
        var projectEvent = context.Message;

        ValidateMessage(projectEvent);
        _plantSetter.SetPlant(projectEvent.Plant);

        if (projectEvent.Behavior == "delete")
        {
            var project = await _projectRepository.GetAsync(projectEvent.ProCoSysGuid, context.CancellationToken);
            _projectRepository.Remove(project);
        }
        
        else if(await _projectRepository.ExistsAsync(projectEvent.ProCoSysGuid, context.CancellationToken))
        {
            var project = await _projectRepository.GetAsync(projectEvent.ProCoSysGuid, context.CancellationToken);
            if (project.ProCoSys4LastUpdated > projectEvent.LastUpdated)
            {
                _logger.LogWarning("Project Message Ignored because a newer LastUpdated already exits in db\n" +
                                       "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                       "EventLastUpdated: {LastUpdated} \n" +
                                       "LastUpdatedFromDb: {ProjectLastUpdated}",
                    context.MessageId, projectEvent.ProCoSysGuid, projectEvent.LastUpdated, project.ProCoSys4LastUpdated);
                return;
            }
            MapFromEventToProject(projectEvent, project);
        }
        else 
        {
            var project = CreateProjectEntity(projectEvent);
            _projectRepository.Add(project);
        }
        _currentUserSetter.SetCurrentUserOid(_applicationOptions.CurrentValue.ObjectId);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("Project Message Consumed: {MessageId} \n Guid {Guid} \n {ProjectName}", 
            context.MessageId, projectEvent.ProCoSysGuid, projectEvent.ProjectName);
    }

    private static void ValidateMessage(ProjectEvent projectEvent)
    {
        if (projectEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception("Message is missing ProCoSysGuid");
        }

        if (string.IsNullOrEmpty(projectEvent.Plant))
        {
            throw new Exception("Message is missing Plant");
        }
    }

    private static void MapFromEventToProject(IProjectEventV1 projectEvent, Project project)
    {
        project.IsClosed = projectEvent.IsClosed;
        project.Name = projectEvent.ProjectName;
        project.SetProCoSys4LastUpdated(projectEvent.LastUpdated);
        
        if (projectEvent.Description is not null)
        {
            project.Description = projectEvent.Description;
        }
    }

    //12/12/2023 We use name as description if description is missing. Its a super edge case and pcs4 does not have any projects with description is null today
    private static Project CreateProjectEntity(IProjectEventV1 projectEvent)
    {
        var project = new Project(projectEvent.Plant,
             projectEvent.ProCoSysGuid,
             projectEvent.ProjectName,
             projectEvent.Description ?? projectEvent.ProjectName);
        project.SetProCoSys4LastUpdated(projectEvent.LastUpdated);
        return project;
    }
}

public record ProjectEvent(string EventType, string? Description, 
    bool IsClosed,
    DateTime LastUpdated,
    string Plant,
    Guid ProCoSysGuid, 
    string ProjectName,
    string? Behavior) 
    : IProjectEventV1;
    
