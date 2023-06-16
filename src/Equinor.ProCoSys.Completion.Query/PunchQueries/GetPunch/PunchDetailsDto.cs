using System;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;

public class PunchDetailsDto
{
    public PunchDetailsDto(
        Guid guid,
        string projectName,
        string title,
        string? text,
        PersonDto createdBy,
        DateTime createdAtUtc,
        PersonDto? modifiedBy,
        DateTime? modifiedAtUtc,
        bool isVoided,
        string rowVersion)
    {
        Guid = guid;
        ProjectName = projectName;
        Title = title;
        Text = text;
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
        ModifiedBy = modifiedBy;
        ModifiedAtUtc = modifiedAtUtc;
        IsVoided = isVoided;
        RowVersion = rowVersion;
    }

    public Guid Guid { get; }
    public string ProjectName { get; }
    public string Title { get; }
    public string? Text { get; }
    public PersonDto CreatedBy { get; }
    public DateTime CreatedAtUtc { get; set; }
    public PersonDto? ModifiedBy { get; }
    public DateTime? ModifiedAtUtc { get; set; }
    public bool IsVoided { get; }
    public string RowVersion { get; }
}
