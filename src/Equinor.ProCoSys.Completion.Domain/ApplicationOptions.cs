#pragma warning disable CS8618
namespace Equinor.ProCoSys.Completion.Domain;

public class ApplicationOptions
{
    public string BaseUrl { get; set; }
    public string ServicePrincipalMail { get; set; }
    public string RejectLabel { get; set; }
    public bool SyncChangesWithPcs4 { get; set; }
    public bool FakeEmail { get; set; }
}
