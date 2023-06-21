using System;
using JetBrains.Annotations;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Punches;

public class PunchDetailsDto
{
    public Guid Guid { get; set; }
    public string ProjectName { get; set; }
    public string ItemNo { get; set; }
    [CanBeNull]
    public string Description { get; set;  }
    public PersonDto CreatedBy { get; set; }
    public string RowVersion { get; set; }
}
