#pragma warning disable CS8618
using System;

namespace Equinor.ProCoSys.Completion.Domain;

public class ApplicationOptions
{
    public string BaseUrl { get; set; }
    public string ServicePrincipalMail { get; set; }
    public string RejectLabel { get; set; }
    public bool RecalculateStatusInPcs4 { get; set; }
    public bool FakeEmail { get; set; }
    public bool DevOnLocalhost { get; set; }
    public Guid ObjectId { get; set; }
    public int CheckListCacheExpirationMinutes { get; set; } = 20;
    public int MaxDuplicatePunch { get; set; } = 50;
    public bool CheckMovedCheckLists { get; set; } = false;
}
