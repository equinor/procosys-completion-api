using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

namespace Equinor.ProCoSys.Completion.Command.EventPublishers.PunchItemEvents;

public interface IPunchEventPublisher
{
    Task<IPunchItemCreatedV1> PublishCreatedEventAsync(
        PunchItem punchItem,
        CancellationToken cancellationToken);

    Task<IPunchItemUpdatedV1> PublishUpdatedEventAsync(
        PunchItem punchItem,
        CancellationToken cancellationToken);

    Task<IPunchItemDeletedV1> PublishDeletedEventAsync(
        PunchItem punchItem,
        CancellationToken cancellationToken);
}
