using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class CommentDto
{
    public Guid SourceGuid { get; set; }
    public Guid Guid { get; set; }
    public string Text { get; set; }
    public PersonDto CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
