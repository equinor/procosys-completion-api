using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.PcsServiceBus.Receiver.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Telemetry;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class BusReceiverService : IBusReceiverService
{
    private readonly IPlantSetter _plantSetter;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IProjectRepository _projectRepository;
    private readonly string _busReceiverTelemetryEvent = "Completion Bus Receiver";

    public BusReceiverService(
        IPlantSetter plantSetter,
        IUnitOfWork unitOfWork,
        ITelemetryClient telemetryClient,
        IProjectRepository projectRepository)
    {
        _plantSetter = plantSetter;
        _unitOfWork = unitOfWork;
        _telemetryClient = telemetryClient;
        _projectRepository = projectRepository;
    }

    public async Task ProcessMessageAsync(string pcsTopic, string messageJson, CancellationToken cancellationToken)
    {
        switch (pcsTopic.ToLower())
        {
            case ProjectTopic.TopicName:
                await ProcessProjectEvent(messageJson);
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessProjectEvent(string messageJson)
    {
        var projectEvent = JsonSerializer.Deserialize<ProjectTopic>(messageJson);
        if (projectEvent is null || projectEvent.Plant.IsEmpty())
        {
            throw new ArgumentNullException($"Deserialized JSON is not a valid ProjectEvent {messageJson}");
        }

        TrackProjectEvent(projectEvent);

        _plantSetter.SetPlant(projectEvent.Plant);

        var project = await _projectRepository.TryGetByGuidAsync(projectEvent.ProCoSysGuid);
        if (project is not null)
        {
            if (projectEvent.Behavior == "delete")
            {
                project.IsDeletedInSource = true;
                return;
            }

            project.Name = projectEvent.ProjectName;
            project.Description = projectEvent.Description;
            project.IsClosed = projectEvent.IsClosed;
        }
        else
        {
            if (projectEvent.Behavior == "delete")
            {
                return;
            }

            project = new Project(
                projectEvent.Plant,
                projectEvent.ProCoSysGuid,
                projectEvent.ProjectName,
                projectEvent.Description);
            _projectRepository.Add(project);
        }
    }

    private void TrackProjectEvent(ProjectTopic projectEvent) =>
        _telemetryClient.TrackEvent(_busReceiverTelemetryEvent,
            new Dictionary<string, string?>
            {
                {"Event", ProjectTopic.TopicName},
                {nameof(projectEvent.Behavior), projectEvent.Behavior},
                {nameof(projectEvent.ProCoSysGuid), projectEvent.ProCoSysGuid.ToString()},
                {nameof(projectEvent.ProjectName), projectEvent.ProjectName},
                {nameof(projectEvent.IsClosed), projectEvent.IsClosed.ToString()},
                {nameof(projectEvent.Plant), projectEvent.Plant[4..]}
            });
}
