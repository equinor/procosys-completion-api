using System;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;

public class PunchDto
{
    public PunchDto(
        Guid guid,
        string projectName,
        string title,
        bool isVoided,
        string rowVersion)
    {
        Guid = guid;
        ProjectName = projectName;
        Title = title;
        IsVoided = isVoided;
        RowVersion = rowVersion;
    }

    public Guid Guid { get; }
    public string ProjectName { get; }
    public string Title { get; }
    public bool IsVoided { get; }
    public string RowVersion { get; }
}
