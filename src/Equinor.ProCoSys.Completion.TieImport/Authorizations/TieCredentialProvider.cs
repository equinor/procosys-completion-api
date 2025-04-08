using Azure.Core;
using Statoil.TI.InterfaceServices.ProxyExtensions;

namespace Equinor.ProCoSys.Completion.TieImport.Authorizations;

public sealed class TieCredentialProvider (TokenCredential tokenCredential) : ITokenProvider
{
    public async Task<AccessToken> GetTokenAsync(TokenRequestContext requestContext) 
        => await tokenCredential.GetTokenAsync(requestContext, default);
}
