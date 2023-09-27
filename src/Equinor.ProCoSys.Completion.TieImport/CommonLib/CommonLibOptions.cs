using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public class CommonLibOptions
{
    [Required]
    public int CacheDurationDays { get; set; }
    [Required]
    public List<string> SchemaFrom { get; set; } = new();
    [Required]
    public string SchemaTo { get; set; } = string.Empty;
    [Required]
    public string ClientId { get; set; } = string.Empty;
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
    [Required]
    public string TenantId { get; set; } = string.Empty;
}
