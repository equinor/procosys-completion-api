using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public sealed class AzureAdOptions
{
    [Required]
    public string ClientId { get; set; } = string.Empty;
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
    [Required]
    public string TenantId { get; set; } = string.Empty;
}
