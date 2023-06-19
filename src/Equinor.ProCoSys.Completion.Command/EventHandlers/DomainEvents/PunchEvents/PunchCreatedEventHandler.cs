using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MassTransit;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;

public class PunchCreatedEventHandler : INotificationHandler<PunchCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    public PunchCreatedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    
    // todo unit test
    public async Task Handle(PunchCreatedEvent notification, CancellationToken cancellationToken) =>
        await _publishEndpoint.Publish<Punch>(new
        {
            SourceGuid = notification.Punch.Guid,
            Title = notification.Punch.Title, 
            CreatedByOid = notification.Punch.CreatedByOid, 
            CreatedAtUtc = notification.Punch.CreatedAtUtc
        }, cancellationToken);

    record Punch : IPunchV1, IPunchV2 //TODO clean up befpre merge. 
    {
        public Guid SourceGuid { get; init; }
        public string? Title { get; init; }
        public Guid CreatedByOid { get; init; }
        public int CreatedById { get; init; }
        
        
        public DateTime CreatedAtUtc { get; init; }
        
    }
    interface IPunchV1
    {
        public Guid SourceGuid { get; init; }
        public string? Title { get; init; }
        public int CreatedById { get; init; }
        
        public DateTime CreatedAtUtc { get; init; }
        
    }

    interface IPunchV2
    {
        public Guid SourceGuid { get; init; }
        public string? Title { get; init; }
        public Guid CreatedByOid { get; init; }
        
        public DateTime CreatedAtUtc { get; init; }
    }
}
