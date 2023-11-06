using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILink
{
    // Guid of the entity owning the Link
    Guid ParentGuid { get; }
    // Type of the entity owning the Link
    string ParentType { get; }
    string Title { get; }
    string Url { get; }
}
