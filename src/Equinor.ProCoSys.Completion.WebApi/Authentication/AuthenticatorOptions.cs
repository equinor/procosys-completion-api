using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Auth.Authentication;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Authentication;

/// <summary>
/// "Mapping" between AzureAdOptions read by IOptionsMonitor to generic IAuthenticatorOptions
/// Needed because keys for configuration differ from application to application
/// </summary>
public class AuthenticatorOptions : IAuthenticatorOptions
{
    private readonly IOptionsMonitor<AzureAdOptions> _azureAdOptions;
    private readonly IDictionary<string, string> _scopes = new Dictionary<string, string>();
        
    public AuthenticatorOptions(IOptionsMonitor<AzureAdOptions> azureAdOptions)
    {
        _azureAdOptions = azureAdOptions;
        var mainApiScope =
            _azureAdOptions.CurrentValue.MainApiScope ??
            throw new ArgumentNullException(
                $"{nameof(AzureAdOptions)}.{nameof(_azureAdOptions.CurrentValue.MainApiScope)} can't be null. Missing configuration?");
        _scopes.Add(MainApiAuthenticator.MainApiScopeKey, mainApiScope);
    }

    public string Authority
        => _azureAdOptions.CurrentValue.Authority ??
           throw new ArgumentNullException(
               $"{nameof(AzureAdOptions)}.{nameof(_azureAdOptions.CurrentValue.Authority)} can't be null. Missing configuration?");

    public string ClientId
        => _azureAdOptions.CurrentValue.ClientId ??
           throw new ArgumentNullException(
               $"{nameof(AzureAdOptions)}.{nameof(_azureAdOptions.CurrentValue.ClientId)} can't be null. Missing configuration?");

    public string ClientSecret
        => _azureAdOptions.CurrentValue.ClientSecret ??
           throw new ArgumentNullException(
               $"{nameof(AzureAdOptions)}.{nameof(_azureAdOptions.CurrentValue.ClientSecret)} can't be null. Missing configuration?");

    public bool DisableRestrictionRoleUserDataClaims
        => _azureAdOptions.CurrentValue.DisableRestrictionRoleUserDataClaims;

    public bool DisableProjectUserDataClaims
        => _azureAdOptions.CurrentValue.DisableProjectUserDataClaims;

    public IDictionary<string, string> Scopes => _scopes;
}
