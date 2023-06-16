using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class AttachmentDto
{
    public Guid SourceGuid { get; set; }
    public Guid Guid { get; set; }
    public string FullBlobPath { get; set; }
    public string FileName { get; set; }
    public PersonDto CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public PersonDto ModifiedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string RowVersion { get; set; }
}
