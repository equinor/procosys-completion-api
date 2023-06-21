using System;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;

public class PunchDto
{
    public PunchDto(
        Guid guid,
        string projectName,
        string itemNo,
        bool isVoided,
        string rowVersion)
    {
        Guid = guid;
        ProjectName = projectName;
        ItemNo = itemNo;
        IsVoided = isVoided;
        RowVersion = rowVersion;
    }

    public Guid Guid { get; }
    public string ProjectName { get; }
    public string ItemNo { get; }
    public bool IsVoided { get; }
    public string RowVersion { get; }
}
