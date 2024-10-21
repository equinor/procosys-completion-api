using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.TieImport;
using Equinor.ProCoSys.Completion.TieImport.Adapter;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.TieImport.Configuration;
using Equinor.ProCoSys.Completion.TieImport.Mocks;
using Equinor.ProCoSys.Completion.TieImport.Services;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Equinor.TI.TIE.Adapter.Base.Message;
using Equinor.TI.TIE.Adapter.Base.Setup;
using Equinor.TI.TIE.Adapter.TIE1.Config;
using Equinor.TI.TIE.Adapter.TIE1.Message;
using Equinor.TI.TIE.Adapter.TIE1.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Client.KeyVaultCertificateReader;
using Statoil.TI.InterfaceServices.ProxyExtensions;

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
        services.AddSingleton<ISchemaSource, CacheWrapper>(_ =>
        {
            var appId = builder.Configuration["AzureAd:ClientId"];
            var tenantId = builder.Configuration["AzureAd:TenantId"];
            var clientSecret = builder.Configuration["AzureAd:ClientSecret"];
            var cacheDuration = builder.Configuration.GetValue<int>("CommonLib:CacheDurationDays");
            ISchemaSource source = new ApiSource(new ApiSourceOptions
            {
                TokenProviderConnectionString = $"RunAs=App;AppId={appId};" +
                                                $"TenantId={tenantId};" +
                                                $"AppKey={clientSecret}"
            });
            return new CacheWrapper(source, maxCacheAge: TimeSpan.FromDays(cacheDuration));
        });
        
        
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

            var tiClientOptions = CreateTiClientOptions(tieImportOptions, azureAdOptions);
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var keyVaultOptions = GetKeyVaultCertificateTokenProviderOptions(tieImportOptions, logger);
            
            services.AddAdapter()
                .WithConfig<TieAdapterConfig, TieAdapterPartitionConfig>()
                .WithStaticConfigRetriever(
                    new TieAdapterConfig
                    {
                        Name = "ProCoSys_Import",
                        Id = "ProCoSys_Import",
                        VerboseLogging = tieImportOptions.VerboseLogging,
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
                    // config.TieErrorShouldLeadToInactivityForAPeriod = LogTieErrorAndReturn(logger,true);
                    // config.TieErrorShouldLeadToInactivityForAPeriodWaitTimeInMs = 10000;
                    config.TieErrorShouldBeThrown = LogTieErrorAndReturn(logger, false);
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

    /// <summary>
    /// I'd like to find a better name, if you have one, feel free to rename
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="boolMethodShouldReturn"></param>
    /// <returns></returns>
    private static Func<ITie1AdapterPartitionConfig, Exception, bool> LogTieErrorAndReturn(ILogger<Program> logger,
        bool boolMethodShouldReturn) =>
        (_, e) =>
        {
            //may not need to log here, as TIE already logs the error
            logger.LogError("TieAdapter threw exception{Exception}",e.Message);
            return boolMethodShouldReturn;
        };

    private static void SetTestSettings(this IServiceCollection services, TieImportOptions configOptions)
    {
        if (configOptions is { TestEnableMockTie1Listener: true, TestEnableTestFileMessageListener: true })
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

    private static TIClientOptions CreateTiClientOptions(
        TieImportOptions configOptions,
        AzureAdOptions azureAdOptions) =>
        new()
        {
            Application = configOptions.AdapterApplication,
            TieUri = configOptions.AdapterTieUri,
            TieId = configOptions.AzureTieApiId,
            ApplicationAzureAppId = azureAdOptions.ClientId,
            ApplicationTenantId = azureAdOptions.TenantId
        };


    private static KeyVaultCertificateTokenProviderOptions GetKeyVaultCertificateTokenProviderOptions(
        TieImportOptions configOptions, ILogger<Program> logger) =>
        new()
        {
            // The KeyVault will be accessed through MSI, so make sure your local user has access policy to read
            // certificates from the KeyVault for development as well as the WebJob/AppService when running in Azure
            KeyVaultUrl = configOptions.AzureKeyVaultUrl,
            Certificate = configOptions.AzureCertificateName,
            ActionOnReadError = ex =>
            {
                logger.LogError("Certificate error: {Exception}", ex.Message);
                return Task.CompletedTask;
            }
        };
}
