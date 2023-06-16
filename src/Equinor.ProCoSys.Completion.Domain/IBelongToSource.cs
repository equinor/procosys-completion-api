using System;

namespace Equinor.ProCoSys.Completion.Domain
{
    public interface IBelongToSource
    {
        string SourceType { get; }
        Guid SourceGuid { get; }
    }
}
