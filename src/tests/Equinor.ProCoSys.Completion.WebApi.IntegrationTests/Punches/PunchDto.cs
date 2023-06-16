using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Punches;

public class PunchDto
{
    public Guid Guid { get; set; }
    public string ProjectName { get; set; }
    public string Title { get; set; }
    public bool IsVoided { get; set; }
    public string RowVersion { get; set; }
}
