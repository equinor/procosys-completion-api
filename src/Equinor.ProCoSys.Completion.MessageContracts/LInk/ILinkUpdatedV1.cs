using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILinkUpdatedV1 : ILink, IIntegrationEvent
{
    User ModifiedBy { get; }
    DateTime ModifiedAtUtc { get; }
    List<IProperty> Changes { get; }
}
