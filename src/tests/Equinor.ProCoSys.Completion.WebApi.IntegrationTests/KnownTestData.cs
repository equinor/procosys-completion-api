using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class KnownTestData
{
    public KnownTestData(string plant) => Plant = plant;

    public string Plant { get; }
    public Guid PunchItemAGuid { get; set; }
    public Guid PunchItemBGuid { get; set; }
    public Guid LinkInPunchItemAGuid { get; set; }
    public Guid CommentInPunchItemAGuid { get; set; }
    public Guid AttachmentInPunchItemAGuid { get; set; }
}
