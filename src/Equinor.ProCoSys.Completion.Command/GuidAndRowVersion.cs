using System;

namespace Equinor.ProCoSys.Completion.Command;

public class GuidAndRowVersion
{
    public GuidAndRowVersion(Guid guid, string rowVersion)
    {
        Guid = guid;
        RowVersion = rowVersion;
    }

    public Guid Guid { get; }
    public string RowVersion { get; }
}
