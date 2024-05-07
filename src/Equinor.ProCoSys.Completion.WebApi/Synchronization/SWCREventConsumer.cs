using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class SWCREventConsumer : IConsumer<SWCREvent>
{
    private readonly ILogger<SWCREventConsumer> _logger;
    private readonly ISWCRRepository _swcrRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserSetter _currentUserSetter;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptions;

    public SWCREventConsumer(ILogger<SWCREventConsumer> logger,
        ISWCRRepository swcrRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserSetter currentUserSetter,
        IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        _logger = logger;
        _swcrRepository = swcrRepository;
        _unitOfWork = unitOfWork;
        _currentUserSetter = currentUserSetter;
        _applicationOptions = applicationOptions;
    }

    public async Task Consume(ConsumeContext<SWCREvent> context)
    {
        var busEvent = context.Message;

        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception("Message is missing ProCoSysGuid");
        }

        if (await _swcrRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var swcr = await _swcrRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            MapFromEventToSWCR(busEvent, swcr);
        }
        else
        {
            var swcr = CreateSWCREntity(busEvent);
            _swcrRepository.Add(swcr);
        }

        _currentUserSetter.SetCurrentUserOid(_applicationOptions.CurrentValue.ObjectId);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Document Message Consumed: {MessageId} \n Guid {Guid} \n {No}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.SwcrNo);

    }

    private static void MapFromEventToSWCR(ISwcrEventV1 swcrEvent, SWCR swcr)
    {
        swcr.IsVoided = swcrEvent.IsVoided;
    }

    private SWCR CreateSWCREntity(ISwcrEventV1 busEvent)
    {
        var swcr = new SWCR(
            busEvent.Plant,
            busEvent.ProCoSysGuid, 
            int.Parse(busEvent.SwcrNo)
        );
        return swcr;
    }
}


public record SWCREvent
(
    string EventType,
    string Plant,
    Guid ProCoSysGuid,
    string? ProjectName,
    string SwcrNo,
    string? Title,
    long SwcrId,
    Guid? CommPkgGuid,
    string? CommPkgNo,
    string? Description,
    string? Modification,
    string? Priority,
    string? System,
    string? ControlSystem,
    string? Contract,
    string? Supplier,
    string? Node,
    string? Status,
    DateTime CreatedAt,
    bool IsVoided,
    DateTime LastUpdated,
    DateOnly? DueDate,
    float? EstimatedManHours
) : ISwcrEventV1;
