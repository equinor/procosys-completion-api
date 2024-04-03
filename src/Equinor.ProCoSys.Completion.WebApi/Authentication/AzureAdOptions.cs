namespace Equinor.ProCoSys.Completion.WebApi.Authentication;

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

    public string? MainApiScope { get; set; }
}
