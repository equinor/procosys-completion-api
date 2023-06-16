using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Auth.Authentication;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Authentication;

/// <summary>
/// "Mapping" between application options read by IOptionsMonitor to generic IAuthenticatorOptions
/// Needed because keys for configuration differ from application to application
/// </summary>
public class AuthenticatorOptions : IAuthenticatorOptions
{
    protected readonly IOptionsMonitor<CompletionAuthenticatorOptions> _options;

    private readonly IDictionary<string, string> _scopes = new Dictionary<string, string>();
        
    public AuthenticatorOptions(IOptionsMonitor<CompletionAuthenticatorOptions> options)
    {
        _options = options;
        var mainApiScope = _options.CurrentValue.MainApiScope ??
                           throw new ArgumentNullException($"{nameof(AuthenticatorOptions)}. {nameof(_options.CurrentValue.MainApiScope)} can't be null. Probably missing configuration");
        _scopes.Add(MainApiAuthenticator.MainApiScopeKey, mainApiScope);
    }

    public string Instance => _options.CurrentValue.Instance ?? 
                              throw new ArgumentNullException($"{nameof(AuthenticatorOptions)}. {nameof(_options.CurrentValue.Instance)} can't be null. Probably missing configuration");

    public string ClientId => _options.CurrentValue.CompletionApiClientId ??
                              throw new ArgumentNullException($"{nameof(AuthenticatorOptions)}. {nameof(_options.CurrentValue.CompletionApiClientId)} can't be null. Probably missing configuration");

    public string Secret => _options.CurrentValue.CompletionApiSecret ??
                            throw new ArgumentNullException($"{nameof(AuthenticatorOptions)}. {nameof(_options.CurrentValue.CompletionApiSecret)} can't be null. Probably missing configuration");

    public Guid ObjectId => _options.CurrentValue.CompletionApiObjectId;

    public bool DisableRestrictionRoleUserDataClaims
        => _options.CurrentValue.DisableRestrictionRoleUserDataClaims;

    public bool DisableProjectUserDataClaims
        => _options.CurrentValue.DisableProjectUserDataClaims;

    public IDictionary<string, string> Scopes => _scopes;
}