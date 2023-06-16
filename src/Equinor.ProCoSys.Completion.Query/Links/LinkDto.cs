using System;

namespace Equinor.ProCoSys.Completion.Query.Links;

public class LinkDto
{
    public LinkDto(Guid sourceGuid, Guid guid, string title, string url, string rowVersion)
    {
        SourceGuid = sourceGuid;
        Guid = guid;
        Title = title;
        Url = url;
        RowVersion = rowVersion;
    }

    public Guid SourceGuid { get; }
    public Guid Guid { get; }
    public string Title { get; }
    public string Url { get; }
    public string RowVersion { get; }
}
