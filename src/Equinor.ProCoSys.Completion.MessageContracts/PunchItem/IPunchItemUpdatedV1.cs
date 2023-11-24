using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItemUpdatedV1 : IPunchItem, IIntegrationEvent
{
    User ModifiedBy { get; }
    DateTime ModifiedAtUtc { get; }
    List<IProperty> Changes { get; }
}
