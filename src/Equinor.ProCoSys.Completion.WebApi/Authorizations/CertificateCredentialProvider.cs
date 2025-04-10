using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public static class CertificateCredentialProvider
{
    /*
     * <summary>
     * Retrieves a certificate from Azure Key Vault and creates a ClientCertificateCredential for authentication.
     * </summary>
     * <param name="configuration">The IConfiguration instance to access application configuration.</param>
     * <param name="tokenCredential">A TokenCredential used to authenticate and access Azure Key Vault.</param>
     * <returns>A Task containing a TokenCredential authenticated using the retrieved certificate.</returns>
     */
    public static async Task<TokenCredential> GetClientCertificateTokenCredential(IConfiguration configuration, string section, TokenCredential tokenCredential)
    {
        // Create a SecretClient to interact with Azure Key Vault using the provided token credential.
        var secretClient = new SecretClient(new Uri(configuration[$"{section}:ClientCredentials:0:KeyVaultUrl"]!), tokenCredential);

        // Retrieve the certificate secret from Key Vault.
        var secret = await secretClient.GetSecretAsync(configuration[$"{section}:ClientCredentials:0:KeyVaultCertificateName"]!);
        // Convert the Base64-encoded certificate into a byte array.
        var certificateBytes = Convert.FromBase64String(secret.Value.Value);
        // Load the certificate into an X509Certificate2 object.
        var certificate = new X509Certificate2(certificateBytes, (string?)null, X509KeyStorageFlags.MachineKeySet);

        // Create a ClientCertificateCredential using the retrieved certificate for authentication.
        return new ClientCertificateCredential(
            configuration[$"{section}:TenantId"],
            configuration[$"{section}:ClientId"],
            certificate);
    }
}
