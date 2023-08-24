//using System.Configuration;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public class CommonLibOptions
{
    public int CacheDurationDays { get; set; }
    public List<string> SchemaFrom { get; set; } = new();
    public string SchemaTo { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}
