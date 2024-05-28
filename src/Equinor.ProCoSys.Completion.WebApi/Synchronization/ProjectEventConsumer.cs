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

public class ProjectEventConsumer(
    ILogger<ProjectEventConsumer> logger,
    IPlantSetter plantSetter,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<ProjectEvent>
{
    public async Task Consume(ConsumeContext<ProjectEvent> context)
    {
        var projectEvent = context.Message;

        ValidateMessage(projectEvent);
        plantSetter.SetPlant(projectEvent.Plant);

        if (projectEvent.Behavior == "delete")
        {
            var project = await projectRepository.GetAsync(projectEvent.ProCoSysGuid, context.CancellationToken);
            projectRepository.Remove(project);
        }
        
        else if(await projectRepository.ExistsAsync(projectEvent.ProCoSysGuid, context.CancellationToken))
        {
            var project = await projectRepository.GetAsync(projectEvent.ProCoSysGuid, context.CancellationToken);

            if (project.ProCoSys4LastUpdated == projectEvent.LastUpdated)
            {
                logger.LogInformation("Project Message Ignored because LastUpdated is the same as in db\n" +
                                   "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                   "EventLastUpdated: {LastUpdated} \n" +
                                   "SyncedToCompletion: {CreatedAtUtc} \n",
                    context.MessageId, projectEvent.ProCoSysGuid, projectEvent.LastUpdated, project.SyncTimestamp );
                return;
            }

            if (project.ProCoSys4LastUpdated > projectEvent.LastUpdated)
            {
                logger.LogWarning("Project Message Ignored because a newer LastUpdated already exits in db\n" +
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
            projectRepository.Add(project);
        }
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("Project Message Consumed: {MessageId} \n Guid {Guid} \n {ProjectName}", 
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
        project.ProCoSys4LastUpdated = projectEvent.LastUpdated;
        project.SyncTimestamp = DateTime.UtcNow;
        
        if (projectEvent.Description is not null)
        {
            project.Description = projectEvent.Description;
        }
    }

    //12/12/2023 We use name as description if description is missing. Its a super edge case and pcs4 does not have any projects with description is null today
    private static Project CreateProjectEntity(IProjectEventV1 projectEvent) =>
        new(projectEvent.Plant,
            projectEvent.ProCoSysGuid,
            projectEvent.ProjectName,
            projectEvent.Description ?? projectEvent.ProjectName)
        {
            IsClosed = projectEvent.IsClosed,
            ProCoSys4LastUpdated = projectEvent.LastUpdated,
            SyncTimestamp = DateTime.UtcNow
        };
}

public record ProjectEvent(string EventType, string? Description, 
    bool IsClosed,
    DateTime LastUpdated,
    string Plant,
    Guid ProCoSysGuid, 
    string ProjectName,
    string? Behavior) 
    : IProjectEventV1;
    
