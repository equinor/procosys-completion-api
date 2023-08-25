using Equinor.TI.TIE.Adapter.Base.Setup;
using Equinor.TI.TIE.Adapter.TIE1.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Statoil.TI.InterfaceServices.Client.KeyVaultCertificateReader;
using Statoil.TI.InterfaceServices.ProxyExtensions;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Adapter;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Configuration;
using Equinor.TI.TIE.Adapter.TIE1.Setup;
using Equinor.ProCoSys.Completion.WebApi.TieImport.Mocks;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using System;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure;

namespace Equinor.ProCoSys.Completion.WebApi.DiModules;

public static class TieImportModule
{
    public static void AddTieImportModule(this IServiceCollection services, IConfiguration configuration)
    {
        var configOptions = new TieImportOptions();
        
        services.AddOptions<TieImportOptions>()
            .BindConfiguration("TieImport")
            .ValidateDataAnnotations()
            .ValidateOnStart(); //TODO: Add required keyword on TieImportOptions class
        configuration.Bind("TieImport", configOptions);
        //services.Configure<TieImportOptions>(configuration.GetSection("TieImport"));

        services.AddOptions<CommonLibOptions>()
            .BindConfiguration("CommonLib")
            .ValidateDataAnnotations()
            .ValidateOnStart(); //TODO: Add required keyword on TieImportOptions class

        //TODO: Scoped or Singleton or Transient?
        services.AddTransient<IImportSchemaMapper, ImportSchemaMapper>();
        services.AddTransient<IMessageInspector, MessageInspector>();
        services.AddAdapterHosting();

        var tiClientOptions = GetTiClientOptions(configOptions);
        var keyVaultOptions = GetKeyVaultCertificateTokenProviderOptions(configOptions);

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

        // Apply test/mock settings, if any
        services.SetTestSettings(configOptions);
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
            services.AddTransient<IMessageListener<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>, TestFileMessageListener<TieAdapterConfig, TieAdapterPartitionConfig>>();
        }
    }

    private static TIClientOptions GetTiClientOptions(TieImportOptions configOptions) =>
        new()
        {
            Application = configOptions.AdapterApplication,
            ApplicationAzureAppId = configOptions.AzureClientId,
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
                //TODO: JSOI - Figure out how to get logger object
                //_logger.LogInformation($"Certificate error: {ex.Message}");
                return Task.CompletedTask;
            }
        };
}
