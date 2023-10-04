using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILink
{
    Guid SourceGuid { get; }
    string SourceType { get; }
    string Title { get; }
    string Url { get; }
}
