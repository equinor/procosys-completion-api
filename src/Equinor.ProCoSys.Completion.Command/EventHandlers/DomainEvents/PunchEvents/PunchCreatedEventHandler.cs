using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MassTransit;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;

public partial class PunchCreatedEventHandler : INotificationHandler<PunchCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PunchCreatedEventHandler(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public async Task Handle(PunchCreatedEvent punchCreatedEvent, CancellationToken cancellationToken)
    {
        // ToDo #103910 Mass Transit logging
        // ToDo #103910 Mass Transit Navn på Topic
        await _publishEndpoint.Publish(new PunchCreatedMessage(punchCreatedEvent),
            context =>
            {
                context.SetSessionId(punchCreatedEvent.Punch.Guid.ToString());
            }, 
            cancellationToken);
    }
}
