using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Command.MessageProducers;

public interface IMessageProducer
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken) where T : class, IIntegrationEvent;
    Task SendHistoryAsync<T>(T message, CancellationToken cancellationToken) where T : class, IHistoryItem;
    Task SendCopyAttachmentEventAsync(AttachmentCopyIntegrationEvent message, CancellationToken cancellationToken);
}
