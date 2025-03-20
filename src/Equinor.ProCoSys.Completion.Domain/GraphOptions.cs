namespace Equinor.ProCoSys.Completion.Domain;

public class GraphOptions
{
    public required string ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TenantId { get; set; }

}
