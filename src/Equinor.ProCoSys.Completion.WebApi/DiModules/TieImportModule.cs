using Equinor.TI.TIE.Adapter.Base.Setup;
using Equinor.TI.TIE.Adapter.TIE1.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Statoil.TI.InterfaceServices.Client.KeyVaultCertificateReader;
using Statoil.TI.InterfaceServices.ProxyExtensions;
using System.Threading.Tasks;
using Equinor.TI.TIE.Adapter.TIE1.Setup;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using System;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.TieImport.Adapter;
using Equinor.ProCoSys.Completion.TieImport;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Configuration;
using Equinor.ProCoSys.Completion.TieImport.Mocks;
using Equinor.ProCoSys.Completion.TieImport.Services;
using Microsoft.AspNetCore.Builder;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class TieImportModule
{
    public static void AddTieImportModule(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddTransient<IImportSchemaMapper, ImportSchemaMapper>();
        services.AddTransient<IImportHandler, ImportHandler>();
        services.AddTransient<IImportDataFetcher, ImportDataFetcher>();
        services.AddTransient<ITiePunchImportService, TiePunchImportService>();
        services.AddScoped<IPunchItemImportService, PunchItemImportService>();
        
        services.AddAdapterHosting();
        if (!builder.Environment.IsIntegrationTest())
        {
            services.AddOptions<TieImportOptions>()
                .BindConfiguration("TieImport")
                .ValidateDataAnnotations();
            var tieImportOptions = new TieImportOptions();
            builder.Configuration.Bind("TieImport", tieImportOptions);

            services.AddOptions<CommonLibOptions>()
                .BindConfiguration("CommonLib")
                .ValidateDataAnnotations();

            services.AddOptions<AzureAdOptions>()
                .BindConfiguration("AzureAd")
                .ValidateDataAnnotations();
            var azureAdOptions = new AzureAdOptions();
            builder.Configuration.Bind("AzureAd", azureAdOptions);

            var tiClientOptions = CreateTiClientOptions(tieImportOptions);
            tiClientOptions.ApplicationAzureAppId = azureAdOptions.ClientId;
            var keyVaultOptions = GetKeyVaultCertificateTokenProviderOptions(tieImportOptions);

            services.AddAdapter()
                .WithConfig<TieAdapterConfig, TieAdapterPartitionConfig>()
                .WithStaticConfigRetriever(
                    new TieAdapterConfig
                    {
                        Name = "ProCoSys_Import",
                        Id = "ProCoSys_Import",
                        VerboseLogging = true,
                        MaxParallellism = 10,
                        ShouldRetrieveFullMessage = true,
                        MessageHandleBehavior = tieImportOptions.AdapterMessageHandleBehavior,
                        MessageChunkSize = tieImportOptions.AdapterMessageChunkSize,
                        IdleTimeBetweenBatch = tieImportOptions.AdapterIdleTimeBetweenBatch,
                        IdleTimeOnNoMessages = tieImportOptions.AdapterIdleTimeOnNoMessages,
                        Partitions = tieImportOptions.AdapterPartitions,
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
                    config.TieErrorShouldBeThrown = (_, _) => true;
                    config.Tie1Info.TokenProvider =
                        new KeyVaultCertificateTokenProvider(tiClientOptions, keyVaultOptions);
                })
                .FromTie1()
                .To<Tie1MessageHandler>()
                .AsBackgroundService()
                .Done();

            // Apply test/mock settings, if any
            services.SetTestSettings(tieImportOptions);
        }
    }

    private static void SetTestSettings(this IServiceCollection services, TieImportOptions configOptions)
    {
        if (configOptions.TestEnableMockTie1Listener &&
            configOptions.TestEnableTestFileMessageListener)
        {
            throw new Exception("TestSettings error: only one MessageListener should be enabled.");
        }

        if (configOptions.TestEnableTestFileMessageListener)
        {
            services
                .AddTransient<IMessageListener<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>,
                    TestFileMessageListener<TieAdapterConfig, TieAdapterPartitionConfig>>();
        }
    }

    private static TIClientOptions CreateTiClientOptions(TieImportOptions configOptions) =>
        new()
        {
            Application = configOptions.AdapterApplication,
            ApplicationTenantId = configOptions.AzureTenantId,
            TieUri = configOptions.AdapterTieUri,
            TieId = configOptions.AzureTieApiId
        };


    private static KeyVaultCertificateTokenProviderOptions GetKeyVaultCertificateTokenProviderOptions(
        TieImportOptions configOptions) =>
        new()
        {
            // The KeyVault will be accessed through MSI, so make sure your local user has access policy to read
            // certificates from the KeyVault for development as well as the WebJob/AppService when running in Azure
            KeyVaultUrl = configOptions.AzureKeyVaultUrl,
            Certificate = configOptions.AzureCertificateName,
            ActionOnReadError = ex =>
            {
                //TODO: 109877 - Figure out how to get logger object
                //_logger.LogInformation($"Certificate error: {ex.Message}");
                return Task.CompletedTask;
            }
        };
}
