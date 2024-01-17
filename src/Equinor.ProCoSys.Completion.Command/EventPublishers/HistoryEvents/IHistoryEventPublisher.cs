using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Command.EventPublishers.HistoryEvents;

public interface IHistoryEventPublisher
{
    Task PublishCreatedEventAsync(
        string plant,
        string displayName,
        Guid guid,
        Guid? parentGuid,
        User createdBy,
        DateTime createdAt,
        List<IProperty> properties,
        CancellationToken cancellationToken);

    Task PublishUpdatedEventAsync(
        string plant,
        string displayName,
        Guid guid,
        User modifiedBy,
        DateTime modifiedAt,
        List<IChangedProperty> changedProperties,
        CancellationToken cancellationToken);

    Task PublishDeletedEventAsync(
        string plant,
        string displayName,
        Guid guid,
        Guid? parentGuid,
        User deletedBy,
        DateTime deletedAt,
        CancellationToken cancellationToken);
}
