using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MassTransit;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;

public class PunchUnvoidedEventHandler : INotificationHandler<PunchUnvoidedEvent>
{
    //private readonly IPublishEndpoint _publishEndpoint;

    // public PunchUnvoidedEventHandler(IPublishEndpoint publishEndpoint)
    // {
    //     //_publishEndpoint = publishEndpoint;
    // }
    // todo unit test
    public  Task Handle(PunchUnvoidedEvent punchUnvoidedEvent, CancellationToken cancellationToken)
    {
        // var sourceGuid = punchUnvoidedEvent.Punch.Guid;
     //  await _publishEndpoint.Publish(punchUnvoidedEvent, cancellationToken);
      return Task.CompletedTask;
    }
}
