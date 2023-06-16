using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Punches;

public class PunchDetailsDto
{
    public Guid Guid { get; set; }
    public string ProjectName { get; set; }
    public string Title { get; set; }
    public string Text { get; set;  }
    public PersonDto CreatedBy { get; set; }
    public bool IsVoided { get; set; }
    public string RowVersion { get; set; }
}
