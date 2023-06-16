using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers;

public class EventDispatcher : IEventDispatcher
{
    private readonly IMediator _mediator;

    public EventDispatcher(IMediator mediator) => _mediator = mediator;

    public async Task DispatchDomainEventsAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default)
    {
        var allEntities = entities.ToList();

        var events = allEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        allEntities.ForEach(e => e.ClearDomainEvents());

        var tasks = PublishToMediator(events, cancellationToken);

        await Task.WhenAll(tasks);
    }

    public async Task DispatchPostSaveEventsAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();

        var events = entityList
            .SelectMany(x => x.PostSaveDomainEvents)
            .ToList();

        entityList.ForEach(e => e.ClearPostSaveDomainEvents());

        var tasks = PublishToMediator(events, cancellationToken);

        await Task.WhenAll(tasks);
    }

    private IEnumerable<Task> PublishToMediator(IList<INotification> domainEvents, CancellationToken cancellationToken)
    {
        var tasks = domainEvents
            .Select(async (domainEvent) =>
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            });
        return tasks;
    }
}
