using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public sealed class CommonLibOptions
{
    [Required]
    public int CacheDurationDays { get; set; }
    [Required]
    public List<string> SchemaFrom { get; set; } = [];
    [Required]
    public string SchemaTo { get; set; } = string.Empty;
    [Required]
    public string Scope { get; set; } = string.Empty;
}
