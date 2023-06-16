using System;

namespace Equinor.ProCoSys.Completion.Command.Links;

public class LinkDto
{
    public LinkDto(Guid guid, string rowVersion)
    {
        Guid = guid;
        RowVersion = rowVersion;
    }

    public Guid Guid { get; }
    public string RowVersion { get; }
}
