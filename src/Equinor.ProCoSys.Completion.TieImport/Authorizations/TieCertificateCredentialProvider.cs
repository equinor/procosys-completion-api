using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Statoil.TI.InterfaceServices.Client.KeyVaultCertificateReader;
using Statoil.TI.InterfaceServices.ProxyExtensions;
using static System.Collections.Specialized.BitVector32;

namespace Equinor.ProCoSys.Completion.TieImport.Authorizations;

public class TieCertificateCredentialProvider : ITokenProvider
{
    private readonly TokenCredential _tokenCredential;
    private readonly TIClientOptions _tieOptions;
    private readonly KeyVaultCertificateTokenProviderOptions _keyVaultOptions;

    public TieCertificateCredentialProvider(
        TokenCredential tokenCredential,
        TIClientOptions tieOptions,
        KeyVaultCertificateTokenProviderOptions keyVaultOptions)
    {
        this._tieOptions = tieOptions;
        this._keyVaultOptions = keyVaultOptions;
        this._tokenCredential = tokenCredential;
    }

    public async Task<AccessToken> GetTokenAsync(TokenRequestContext requestContext)
    {
        //var tieCredential = await GetClientCertificateTokenCredential();
        //var tmp = tieCredential.GetTokenAsync(requestContext, default).GetAwaiter().GetResult();

        var cred = new ClientSecretCredential(
            "3aa4a235-b6e2-48d5-9195-7fcf05b459b0",
            "cf2e4cc0-39fe-4604-ad64-98e392987821",
            "");

        var tmp = cred.GetTokenAsync(requestContext, default).GetAwaiter().GetResult();

        return _tokenCredential.GetTokenAsync(requestContext, default).GetAwaiter().GetResult();
    }

    public async Task<TokenCredential> GetClientCertificateTokenCredential()
    {
        // Create a SecretClient to interact with Azure Key Vault using the provided token credential.
        var secretClient = new SecretClient(new Uri(_keyVaultOptions.KeyVaultUrl), _tokenCredential);

        // Retrieve the certificate secret from Key Vault.
        var secret = await secretClient.GetSecretAsync(_keyVaultOptions.Certificate);
        // Convert the Base64-encoded certificate into a byte array.
        var certificateBytes = Convert.FromBase64String(secret.Value.Value);
        // Load the certificate into an X509Certificate2 object.
        var certificate = new X509Certificate2(certificateBytes, (string?)null, X509KeyStorageFlags.MachineKeySet);

        // Create a ClientCertificateCredential using the retrieved certificate for authentication.
        var res =  new ClientCertificateCredential(
            _tieOptions.ApplicationTenantId,
            _tieOptions.ApplicationAzureAppId,
            certificate);
        return res;
    }
}
