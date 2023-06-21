using System;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;

public class PunchDto
{
    public PunchDto(
        Guid guid,
        string projectName,
        string itemNo,
        string rowVersion)
    {
        Guid = guid;
        ProjectName = projectName;
        ItemNo = itemNo;
        RowVersion = rowVersion;
    }

    public Guid Guid { get; }
    public string ProjectName { get; }
    public string ItemNo { get; }
    public string RowVersion { get; }
}
