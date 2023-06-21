using System;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;

public class PunchDetailsDto
{
    public PunchDetailsDto(
        Guid guid,
        string projectName,
        string itemNo,
        string? description,
        PersonDto createdBy,
        DateTime createdAtUtc,
        PersonDto? modifiedBy,
        DateTime? modifiedAtUtc,
        string rowVersion)
    {
        Guid = guid;
        ProjectName = projectName;
        ItemNo = itemNo;
        Description = description;
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
        ModifiedBy = modifiedBy;
        ModifiedAtUtc = modifiedAtUtc;
        RowVersion = rowVersion;
    }

    public Guid Guid { get; }
    public string ProjectName { get; }
    public string ItemNo { get; }
    public string? Description { get; }
    public PersonDto CreatedBy { get; }
    public DateTime CreatedAtUtc { get; set; }
    public PersonDto? ModifiedBy { get; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string RowVersion { get; }
}
