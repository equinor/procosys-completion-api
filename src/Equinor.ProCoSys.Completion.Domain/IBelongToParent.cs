using System;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IBelongToParent
{
    string ParentType { get; }
    Guid ParentGuid { get; }
}
