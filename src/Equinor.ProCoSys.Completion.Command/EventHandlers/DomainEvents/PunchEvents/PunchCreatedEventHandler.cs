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

    // todo unit test to fail if property in contract IPunchCreatedV1 not in published message
    public async Task Handle(PunchCreatedEvent punchCreatedEvent, CancellationToken cancellationToken) =>
        await _publishEndpoint.Publish(new PunchCreatedMessage(punchCreatedEvent), cancellationToken);

    public record PunchCreatedMessage : IPunchCreatedV1
    {
        public PunchCreatedMessage(PunchCreatedEvent punchCreatedEvent)
        {
            ProjectGuid = punchCreatedEvent.ProjectGuid;
            Guid = punchCreatedEvent.Punch.Guid;
            Title = punchCreatedEvent.Punch.Title;
            CreatedAtUtc = punchCreatedEvent.Punch.CreatedAtUtc;
            CreatedByOid = punchCreatedEvent.Punch.CreatedByOid;
        }

        public string DisplayName => "Punch created";

        public Guid ProjectGuid { get; }
        public Guid Guid { get; init; }
        public string Title { get; init; }
        public Guid CreatedByOid { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }
}
