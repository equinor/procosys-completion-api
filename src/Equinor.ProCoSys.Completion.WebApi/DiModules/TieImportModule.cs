using AdapterConsoleApp.Configuration;
using Equinor.TI.TIE.Adapter.Base.Setup;
using Equinor.TI.TIE.Adapter.TIE1.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Statoil.TI.InterfaceServices.Client.KeyVaultCertificateReader;
using Statoil.TI.InterfaceServices.ProxyExtensions;
using System.Threading.Tasks;
using AdapterConsoleApp.Adapter;
using Equinor.TI.TIE.Adapter.TIE1.Setup;

namespace Equinor.ProCoSys.Completion.WebApi.DiModules
{
    public static class TieImportModule
    {
        public static void AddTieImportModule(this IServiceCollection services, IConfiguration configuration)
        {
            var configOptions = new TieImportOptions();
            configuration.Bind("TieImport", configOptions);

            services.AddAdapterHosting();

            // TIE authentication config
            var tiClientOptions = GetTiClientOptions(configOptions);
            var keyVaultOptions = GetKeyVaultCertificateTokenProviderOptions(configOptions);

            services.AddAdapter()  // From TieAdapter NuGet
                .WithConfig<TieAdapterConfig, TieAdapterPartitionConfig>()
                .WithStaticConfigRetriever(
                    new TieAdapterConfig
                    {
                        Name = "ProCoSys_Import",
                        Id = "ProCoSys_Import",
                        VerboseLogging = true,
                        MaxParallellism = 10,
                        ShouldRetrieveFullMessage = true,
                        MessageHandleBehavior = configOptions.AdapterMessageHandleBehavior,
                        MessageChunkSize = configOptions.AdapterMessageChunkSize,
                        IdleTimeBetweenBatch = configOptions.AdapterIdleTimeBetweenBatch,
                        IdleTimeOnNoMessages = configOptions.AdapterIdleTimeOnNoMessages,
                        Partitions = configOptions.AdapterPartitions,
                        Tie1Info = new Tie1Info
                        {
                            ClientOptions = tiClientOptions,
                            UseDefaultProvider = false,
                            TokenProvider = null // --> see .WithConfigModifier()
                        }
                    }
                )
                .WithConfigModifier(config =>
                {
                    config.TieErrorShouldBeThrown = (c, ex) => true;
                    config.Tie1Info.TokenProvider =
                        new KeyVaultCertificateTokenProvider(tiClientOptions, keyVaultOptions);
                })
                .FromTie1()
                .To<Tie1MessageHandler>()
                .AsBackgroundService()
                .Done();
        }

        private static TIClientOptions GetTiClientOptions(TieImportOptions configOptions) =>
            new()
            {
                // The application/source system you want to send in data on behalf of.
                // This matches the user name/ALIAS name previously used in TIE1
                Application = configOptions.AdapterApplication,

                // The application id / "client id" of the application registration for your principal.
                ApplicationAzureAppId = configOptions.AzureClientId,

                // Equinor Azure AD tenant ID.
                ApplicationTenantId = configOptions.AzureTenantId,

                // The uri to the TIE 1.5 API. Either https://qa-tie-proxy.equinor.com or https://tie-proxy.equinor.com
                TieUri = configOptions.AdapterTieUri,

                // The id of the TIE 1.5 API.
                // 246de5ab-6c09-4df7-aaab-370df915deea for the QA environment
                // 95e98a4a-840e-4209-bd03-411e03d475b4 for the production environment 
                TieId = configOptions.AzureTieApiId
            };


        private static KeyVaultCertificateTokenProviderOptions GetKeyVaultCertificateTokenProviderOptions(
            TieImportOptions configOptions) =>
            new()
            {
                // Url to your Azure KeyVault.
                // The KeyVault will be accessed through MSI, so make sure your local user has access policy to read
                // certificates from the KeyVault for development as well as the WebJob/AppService when running in Azure
                KeyVaultUrl = configOptions.AzureKeyVaultUrl,

                // The certificate name
                Certificate = configOptions.AzureCertificateName,

                // Optional action if the provider fails to read the certificate
                // Optional action if the provider fails to read the certificate
                ActionOnReadError = ex =>
                {
                    //TODO: JSOI - Figure out how to get logger object
                    //_logger.LogInformation($"Certificate error: {ex.Message}");
                    return Task.CompletedTask;
                }
            };
    }
}
