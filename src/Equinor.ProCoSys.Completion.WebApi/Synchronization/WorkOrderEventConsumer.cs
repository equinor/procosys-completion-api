using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class WorkOrderEventConsumer : IConsumer<WorkOrderEvent>
{
    private readonly ILogger<WorkOrderEventConsumer> _logger;
    private readonly IPlantSetter _plantSetter;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptions;

    public WorkOrderEventConsumer(ILogger<WorkOrderEventConsumer> logger,
        IPlantSetter plantSetter,
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserSetter currentUserSetter,
        IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        _logger = logger;
        _plantSetter = plantSetter;
        _workOrderRepository = workOrderRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _applicationOptions = applicationOptions;
    }

    public async Task Consume(ConsumeContext<WorkOrderEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);
        _plantSetter.SetPlant(busEvent.Plant);

        if (await _workOrderRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var workOrder = await _workOrderRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            MapFromEventToWorkOrder(busEvent, workOrder);
        }
        else
        {
            var workOrder = CreateWorkOrderEntity(busEvent);
            _workOrderRepository.Add(workOrder);
        }

        _currentUserSetter.SetCurrentUserOid(_applicationOptions.CurrentValue.ObjectId);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation($"{nameof(WorkOrderEvent)} Message Consumed: {{MessageId}} \n Guid {{Guid}} \n No {{No}}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.WoNo);
    }

    private static void ValidateMessage(WorkOrderEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(WorkOrderEvent)} is missing {nameof(WorkOrderEvent.ProCoSysGuid)}");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception($"{nameof(WorkOrderEvent)} is missing {nameof(WorkOrderEvent.Plant)}");
        }
    }

    private static void MapFromEventToWorkOrder(IWorkOrderEventV1 busEvent, WorkOrder workOrder)
    {
        workOrder.No = busEvent.WoNo;
        // TODO Investigate mapping for isClosed/isVoided
        // workOrder.IsClosed = busEvent.IsVoided;
    }

    private static WorkOrder CreateWorkOrderEntity(IWorkOrderEventV1 busEvent) => new (
            busEvent.Plant,
            busEvent.ProCoSysGuid,
            busEvent.WoNo
        );

}

public record WorkOrderEvent
(
    string EventType,
    string Plant,
    Guid ProCoSysGuid,
    string ProjectName,
    string WoNo,
    long WoId,
    string? CommPkgNo,
    Guid? CommPkgGuid,
    string? Title,
    string? Description,
    string? MilestoneCode,
    string? SubMilestoneCode,
    string? MilestoneDescription,
    string? CategoryCode,
    string? MaterialStatusCode,
    string? HoldByCode,
    string? DisciplineCode,
    string? DisciplineDescription,
    string? ResponsibleCode,
    string? ResponsibleDescription,
    string? AreaCode,
    string? AreaDescription,
    string? JobStatusCode,
    string? MaterialComments,
    string? ConstructionComments,
    string? TypeOfWorkCode,
    string? OnShoreOffShoreCode,
    string? WoTypeCode,
    double ProjectProgress,
    int Progress,
    string? ExpendedManHours,
    string? EstimatedHours,
    string? RemainingHours,
    string? WBS,
    DateOnly? PlannedStartAtDate,
    DateOnly? ActualStartAtDate,
    DateOnly? PlannedFinishedAtDate,
    DateOnly? ActualFinishedAtDate,
    DateTime CreatedAt,
    bool IsVoided,
    DateTime LastUpdated
) : IWorkOrderEventV1;
