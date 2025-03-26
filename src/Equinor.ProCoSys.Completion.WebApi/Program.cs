using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.DIModules;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using StackExchange.Redis;

const string AllowAllOriginsCorsPolicy = "AllowAllOrigins";

var builder = WebApplication.CreateBuilder(args);
var devOnLocalhost = builder.Configuration.IsDevOnLocalhost();

// Use DefaultAzureCredential to authenticate with Azure.  
// If running locally (devOnLocalhost == true), allow interactive login methods  
// (e.g., Visual Studio, Azure CLI, and interactive browser prompts).  
// In production, interactive credentials are disabled for automated authentication.
TokenCredential credential = new DefaultAzureCredential(includeInteractiveCredentials: devOnLocalhost);

// If running locally (devOnLocalhost == true), retrieve a certificate-based credential  
// from Azure Key Vault and use it for authentication.  
// This ensures the application mimics production authentication while running in a local environment.  
if (devOnLocalhost)
{
    credential = await GetCertificateTokenCredential(builder, credential);
}

builder.Services.AddSingleton(credential);
builder.ConfigureAzureAppConfig(credential);
builder.Services.AddHealthChecks();

// Don't use Redis for integration test. Use the memory implementation
if (!builder.Environment.IsIntegrationTest())
{
    var redisHostName = builder.Configuration["Redis:HostName"];
    var configurationOptions = ConfigurationOptions.Parse($"{redisHostName}");
    configurationOptions.AbortOnConnectFail = false;

    await configurationOptions.ConfigureForAzureWithTokenCredentialAsync(credential);

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConnectionMultiplexerFactory = async () => 
            await ConnectionMultiplexer.ConnectAsync(configurationOptions);
    });
}
else if (builder.Environment.IsIntegrationTest())
{
    builder.Services.AddDistributedMemoryCache();
}

builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = null;
});


builder.ConfigureHostedJobs(devOnLocalhost);
builder.ConfigureSwagger();
builder.ConfigureHttp();

//TODO: PBI #104224 "Ensure using Auth Code Grant flow and add token validation"
builder.Services
    .AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDistributedTokenCaches();

builder.Services.AddPcsAuthIntegration();

builder.ConfigureValidators();
builder.ConfigureTelemetry(credential, devOnLocalhost);

builder.Services.AddMediatrModules();
builder.Services.AddApplicationModules(builder.Configuration, credential);

if (builder.Configuration.GetValue<bool>("TieImport:Enable"))
{
    builder.Services.AddTieImportModule(builder);
}

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("UseAzureAppConfiguration"))
{
    app.UseAzureAppConfiguration();
}

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseGlobalExceptionHandling();

app.UseCors(AllowAllOriginsCorsPolicy); //TODO: #104225 "CORS - Use a list of clients, not AllowAll"

app.UseCompletionSwagger(builder.Configuration);

app.UseHttpsRedirection();

app.UseRouting();

// order of adding middleware are crucial. Some depend that other has been run in advance
app.UseCurrentPlant();
app.UseAuthentication();
app.UseCurrentUser();
app.UsePersonValidator();
app.UsePlantValidator();
app.UseVerifyOidInDb();
app.UseAuthorization();

app.UseResponseCompression();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
return;

/*
 * <summary>
 * Retrieves a certificate from Azure Key Vault and creates a ClientCertificateCredential for authentication.
 * </summary>
 * <param name="webApplicationBuilder">The WebApplicationBuilder instance to access application configuration.</param>
 * <param name="tokenCredential">A TokenCredential used to authenticate and access Azure Key Vault.</param>
 * <returns>A Task containing a TokenCredential authenticated using the retrieved certificate.</returns>
*/
async Task<TokenCredential> GetCertificateTokenCredential(WebApplicationBuilder webApplicationBuilder, TokenCredential tokenCredential)
{
    // Create a SecretClient to interact with Azure Key Vault using the provided token credential.
    var secretClient = new SecretClient(new Uri(webApplicationBuilder.Configuration["AzureAd:ClientCredentials:0:KeyVaultUrl"]!), tokenCredential);

    // Retrieve the certificate secret from Key Vault.
    var secret = await secretClient.GetSecretAsync(webApplicationBuilder.Configuration["AzureAd:ClientCredentials:0:KeyVaultCertificateName"]!);
    // Convert the Base64-encoded certificate into a byte array.
    var certificateBytes = Convert.FromBase64String(secret.Value.Value);
    // Load the certificate into an X509Certificate2 object.
    var certificate = new X509Certificate2(certificateBytes, (string?)null, X509KeyStorageFlags.MachineKeySet);

    // Create a ClientCertificateCredential using the retrieved certificate for authentication.
    return new ClientCertificateCredential(
        webApplicationBuilder.Configuration["AzureAd:TenantId"],
        webApplicationBuilder.Configuration["AzureAd:ClientId"],
        certificate);
}

public partial class Program { }
