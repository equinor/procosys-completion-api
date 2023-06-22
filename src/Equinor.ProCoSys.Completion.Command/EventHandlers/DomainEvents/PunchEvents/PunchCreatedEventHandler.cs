using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.MessageContracts.Punch;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MassTransit;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;

public class PunchCreatedEventHandler : INotificationHandler<PunchCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PunchCreatedEventHandler(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public async Task Handle(PunchCreatedEvent punchCreatedEvent, CancellationToken cancellationToken)
    {
        // ToDo #103910 Mass Transit logging
        // ToDo #103910 Mass Transit Navn på Topic
        await _publishEndpoint.Publish(new PunchCreatedMessage(punchCreatedEvent), cancellationToken);
    }

   private record PunchCreatedMessage : IPunchCreatedV1
    {
        internal PunchCreatedMessage(PunchCreatedEvent punchCreatedEvent)
        {
            ProjectGuid = punchCreatedEvent.ProjectGuid;
            Guid = punchCreatedEvent.Punch.Guid;
            ItemNo = punchCreatedEvent.Punch.ItemNo;
            CreatedAtUtc = punchCreatedEvent.Punch.CreatedAtUtc;
            CreatedByOid = punchCreatedEvent.Punch.CreatedByOid;
        }

        public string DisplayName => "Punch created";

        public Guid ProjectGuid { get; }
        public Guid Guid { get; init; }
        public string ItemNo { get; init; }
        public Guid CreatedByOid { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }
}
