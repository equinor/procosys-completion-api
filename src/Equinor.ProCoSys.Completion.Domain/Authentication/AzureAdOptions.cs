namespace Equinor.ProCoSys.Completion.Domain.Authentication;

/// <summary>
/// Options for Azure authentication and authorization.
/// Read from application configuration via IOptionsMonitor.
/// "Mapped" to the generic IAuthenticatorOptions
/// </summary>
public class AzureAdOptions
{
    public string? Authority { get; set; }

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    public bool DisableProjectUserDataClaims { get; set; }
    public bool DisableRestrictionRoleUserDataClaims { get; set; }

    public required string MainApiScope { get; set; }
}
