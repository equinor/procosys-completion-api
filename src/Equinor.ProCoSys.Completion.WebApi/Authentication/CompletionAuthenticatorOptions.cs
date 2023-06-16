using System;

namespace Equinor.ProCoSys.Completion.WebApi.Authentication;

/// <summary>
/// Options for Authentication. Read from application configuration via IOptionsMonitor.
/// "Mapped" to the generic IAuthenticatorOptions
/// </summary>
public class CompletionAuthenticatorOptions
{
    public string? Instance { get; set; }

    public string? CompletionApiClientId { get; set; }
    public string? CompletionApiSecret { get; set; }
    public Guid CompletionApiObjectId { get; set; }

    public bool DisableProjectUserDataClaims { get; set; }
    public bool DisableRestrictionRoleUserDataClaims { get; set; }

    public string? MainApiScope { get; set; }
}
