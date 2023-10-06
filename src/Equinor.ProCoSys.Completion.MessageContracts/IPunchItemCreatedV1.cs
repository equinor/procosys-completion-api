using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemCreatedV1 : IPunchItem, IIntegrationEvent
{
    Guid CreatedByOid { get; }
    DateTime CreatedAtUtc { get;  }
}
