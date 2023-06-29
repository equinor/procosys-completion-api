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

    /// <summary>
    /// Asynchronously dispatches domain events associated with a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of entities that may have domain events to dispatch.</param>
    /// <param name="cancellationToken">(Optional) A cancellation token that can be used to cancel the operation.</param>
    /// <remarks>
    /// This method should be called BEFORE saveAsync to ensure that all operations are performed within the same transaction.
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation of dispatching domain events.</returns>
    public async Task DispatchDomainEventsAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default)
    {
        var allEntities = entities.ToList();
        
        var domainEvents = allEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        allEntities.ForEach(e => e.ClearDomainEvents());
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Asynchronously dispatches domain events associated with a collection of entities.
    /// </summary>
    /// <param name="entities">The collection of entities that may have domain events to dispatch.</param>
    /// <param name="cancellationToken">(Optional) A cancellation token that can be used to cancel the operation.</param>
    /// <remarks>
    /// Should be called Right AFTER committing data (EF SaveChanges) and will cause multiple transactions. 
    /// You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation of dispatching domain events.</returns>
    public async Task DispatchPostSaveEventsAsync(IEnumerable<EntityBase> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();

        var events = entityList
            .SelectMany(x => x.PostSaveDomainEvents)
            .ToList();

        entityList.ForEach(e => e.ClearPostSaveDomainEvents());

        await PublishEvents(events, cancellationToken);
    }

    private async Task PublishEvents(IEnumerable<INotification> events, CancellationToken cancellationToken)
    {
        var tasks = events.Select(domainEvent 
            => _mediator.Publish(domainEvent, cancellationToken));
    
        await Task.WhenAll(tasks);
    }
}
