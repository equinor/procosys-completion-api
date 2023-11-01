using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILinkCreatedV1 : ILink, IIntegrationEvent
{
    IUser CreatedBy { get; }
    DateTime CreatedAtUtc { get;  }
}
