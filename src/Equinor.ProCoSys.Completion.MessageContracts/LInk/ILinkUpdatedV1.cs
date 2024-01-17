using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILinkUpdatedV1 : ILink, IIntegrationEvent
{
    User ModifiedBy { get; }
    DateTime ModifiedAtUtc { get; }
    List<IChangedProperty> Changes { get; }
}
