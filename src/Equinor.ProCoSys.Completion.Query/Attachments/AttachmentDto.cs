using System;

namespace Equinor.ProCoSys.Completion.Query.Attachments;

public class AttachmentDto
{
    public AttachmentDto(
        Guid sourceGuid,
        Guid guid,
        string fullBlobPath,
        string fileName,
        PersonDto createdBy,
        DateTime createdAtUtc,
        PersonDto? modifiedBy,
        DateTime? modifiedAtUtc,
        string rowVersion)
    {
        SourceGuid = sourceGuid;
        Guid = guid;
        FullBlobPath = fullBlobPath;
        FileName = fileName;
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
        ModifiedBy = modifiedBy;
        ModifiedAtUtc = modifiedAtUtc;
        RowVersion = rowVersion;
    }

    public Guid SourceGuid { get; }
    public Guid Guid { get; }
    public string FullBlobPath { get; }
    public string FileName { get; }
    public PersonDto CreatedBy { get; }
    public DateTime CreatedAtUtc { get; set; }
    public PersonDto? ModifiedBy { get; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string RowVersion { get; }
}
