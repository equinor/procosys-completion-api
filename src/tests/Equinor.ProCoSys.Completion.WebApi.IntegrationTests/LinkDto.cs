using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class LinkDto
{
    public Guid SourceGuid { get; set; }
    public Guid Guid { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string RowVersion { get; set; }
}
