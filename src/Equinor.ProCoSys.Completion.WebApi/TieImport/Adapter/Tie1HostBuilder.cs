//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Threading.Tasks;
//using AdapterConsoleApp.ApplicationInsights;
//using AdapterConsoleApp.Configuration;
//using AdapterConsoleApp.Mocks;
//using Azure.Identity;
//using Equinor.TI.TIE.Adapter.Base.Message;
//using Equinor.TI.TIE.Adapter.Base.Setup;
//using Equinor.TI.TIE.Adapter.TIE1.Config;
//using Equinor.TI.TIE.Adapter.TIE1.Message;
//using Equinor.TI.TIE.Adapter.TIE1.Setup;
//using log4net;
//using Microsoft.ApplicationInsights.Extensibility;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.AzureAppConfiguration;
//using Microsoft.Extensions.Configuration.AzureAppConfiguration.FeatureManagement;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.FeatureManagement;
//using Statoil.TI.InterfaceServices.Client.KeyVaultCertificateReader;
//using Statoil.TI.InterfaceServices.ProxyExtensions;
//using ConfigurationManager = System.Configuration.ConfigurationManager;

//namespace AdapterConsoleApp.Adapter
//{
//    public static class Tie1HostBuilder
//    {
//        private static readonly ILog _logger = LogManager.GetLogger(typeof(Tie1HostBuilder));

//        public static IHostBuilder CreateHostBuilder(string[] args)
//        {
//            var hostBuilder = Host.CreateDefaultBuilder(args)
//                .ConfigureAppConfiguration((context, config) =>
//                {
//                     // From app.config
//                     config.AddInMemoryCollection(GetAppSettingKeyValues());

//                     config.AddAzureAppConfiguration(
//                            options =>
//                            {
//                                var appConfigurationName = ConfigurationManager.AppSettings["Azure:AppConfig"];
//                                var endpoint = $"https://{appConfigurationName}.azconfig.io";

//                                options.Connect(new Uri(endpoint), new DefaultAzureCredential())
//                                    .ConfigureKeyVault(
//                                        kv =>
//                                        {
//                                            kv.SetCredential(new DefaultAzureCredential());
//                                        })
//                                    .Select(KeyFilter.Any, ConfigurationManager.AppSettings["InstanceName"])
//                                    .Select(
//                                        KeyFilter.Any,
//                                        ConfigurationManager.AppSettings["Azure:AppConfigLabelFilter"]);
//                                options.UseFeatureFlags((Action<FeatureFlagOptions>)(opt => opt.Label = ConfigurationManager.AppSettings["Azure:AppConfigLabelFilter"]));
//                            });



//                    // Need to explicitly update values in ConfigurationManager, possible reason is some limitations to
//                    // how Azure App Configuration works with .NET console application running as WebJob
//                    UpdateConfigurationManager(config.Build());
//                    ConfigureApplicationInsights();
//                    UpdateConnectionString();
//                })
//                .ConfigureServices((hostContext, services) =>
//                {
//                    services.AddLogging(config =>
//                    {
//                        config.ClearProviders();
//                        config.AddLog4Net();
//                    });

//                    // Inject settings from app.config
//                    var configOptions = new ConfigurationOptions();
//                    hostContext.Configuration.Bind(configOptions);
//                    services.Configure<ConfigurationOptions>(c => hostContext.Configuration.Bind(c));

//                    services.AddTransient<ITelemetryHelper, TelemetryHelper>();

//                    services.AddAdapterHosting();

//                    // TIE authentication config
//                    var tiClientOptions = GetTiClientOptions(configOptions);
//                    var keyVaultOptions = GetKeyVaultCertificateTokenProviderOptions(configOptions);
//                    services.AddFeatureManagement();

//                    services.AddAdapter()
//                        .WithConfig<TieAdapterConfig, TieAdapterPartitionConfig>()
//                        .WithStaticConfigRetriever(
//                            new TieAdapterConfig
//                            {
//                                Name = "ProCoSys_Import",
//                                Id = "ProCoSys_Import",
//                                VerboseLogging = true,
//                                MaxParallellism = 10,
//                                ShouldRetrieveFullMessage = true,
//                                MessageHandleBehavior = configOptions.AdapterMessageHandleBehavior,
//                                MessageChunkSize = configOptions.AdapterMessageChunkSize,
//                                IdleTimeBetweenBatch = configOptions.AdapterIdleTimeBetweenBatch,
//                                IdleTimeOnNoMessages = configOptions.AdapterIdleTimeOnNoMessages,
//                                Partitions = configOptions.AdapterPartitions,
//                                Tie1Info = new Tie1Info
//                                {
//                                    ClientOptions = tiClientOptions,
//                                    UseDefaultProvider = false,
//                                    TokenProvider = null // --> see .WithConfigModifier()
//                                }
//                            }
//                        )
//                        .WithConfigModifier(config =>
//                        {
//                            config.TieErrorShouldBeThrown = (c, ex) => true;
//                            config.Tie1Info.TokenProvider =
//                                new KeyVaultCertificateTokenProvider(tiClientOptions, keyVaultOptions);
//                        })
//                        .FromTie1()
//                        .To<Tie1MessageHandler>()
//                        .AsBackgroundService()
//                        .Done();

//                    SetFeatureFlagsInConfig(services);

//                    // Apply test/mock settings, if any
//                    services.SetTestSettings(configOptions);

//                    // Log config summary
//                    _logger.Info($"Using the following configuration options:{Environment.NewLine}{configOptions}");
//                });

//            return hostBuilder;
//        }

//        private static void SetFeatureFlagsInConfig(IServiceCollection services)
//        {
//            //See commit 8b9d1cb3e9071f2823a986e4a05df8c041778458 from 01.09.2022 for example on how to add feature flag
//        }

//        private static TIClientOptions GetTiClientOptions(ConfigurationOptions configOptions) =>
//            new TIClientOptions
//            {
//                // The application/source system you want to send in data on behalf of.
//                // This matches the user name/ALIAS name previously used in TIE1
//                Application = configOptions.AdapterApplication,

//                // The application id / "client id" of the application registration for your principal.
//                ApplicationAzureAppId = configOptions.AzureClientId,

//                // Equinor Azure AD tenant ID.
//                ApplicationTenantId = configOptions.AzureTenantId,

//                // The uri to the TIE 1.5 API. Either https://qa-tie-proxy.equinor.com or https://tie-proxy.equinor.com
//                TieUri = configOptions.AdapterTieUri,

//                // The id of the TIE 1.5 API.
//                // 246de5ab-6c09-4df7-aaab-370df915deea for the QA environment
//                // 95e98a4a-840e-4209-bd03-411e03d475b4 for the production environment 
//                TieId = configOptions.AzureTieApiId
//            };

//        private static KeyVaultCertificateTokenProviderOptions GetKeyVaultCertificateTokenProviderOptions(
//            ConfigurationOptions configOptions) =>
//            new KeyVaultCertificateTokenProviderOptions
//            {
//                // Url to your Azure KeyVault.
//                // The KeyVault will be accessed through MSI, so make sure your local user has access policy to read
//                // certificates from the KeyVault for development as well as the WebJob/AppService when running in Azure
//                KeyVaultUrl = configOptions.AzureKeyVaultUrl,

//                // The certificate name
//                Certificate = configOptions.AzureCertificateName,

//                // Optional action if the provider fails to read the certificate
//                // Optional action if the provider fails to read the certificate
//                ActionOnReadError = ex =>
//                {
//                    _logger.Info($"Certificate error: {ex.Message}");
//                    return Task.CompletedTask;
//                }
//            };

//         private static void UpdateConfigurationManager(IConfiguration config)
//         {
//            foreach (var keyValuePair in config.AsEnumerable())
//            {
//                ConfigurationManager.AppSettings[keyValuePair.Key] = keyValuePair.Value;
//            }
//         }

//         private static void UpdateConnectionString()
//         {
//             var connectionString = ConfigurationManager.AppSettings["OracleConnectionString"];
//             var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

//             config.ConnectionStrings.ConnectionStrings["ProcosysDB"].ConnectionString = connectionString;
//             config.Save(ConfigurationSaveMode.Modified);

//             ConfigurationManager.RefreshSection("connectionStrings");
//         }

//        private static void ConfigureApplicationInsights()
//        {
//            // Set up instrumentation key for ApplicationInsights
//            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["InstrumentationKey"];

//#if DEBUG
//            // Disable AI tracking in Debug mode
//            TelemetryConfiguration.Active.DisableTelemetry = true;
//#endif
//        }

//        private static void SetTestSettings(this IServiceCollection services, ConfigurationOptions configOptions)
//        {
//            if (configOptions.TestEnableMockTie1Listener &&
//                configOptions.TestEnableTestFileMessageListener)
//            {
//                throw new Exception("TestSettings error: only one MessageListener should be enabled.");
//            }

//            // Override default Tie1 message listener for testing purposes
//            if (configOptions.TestEnableMockTie1Listener)
//            {
//                services.AddTransient<IMessageListener<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>, MockTie1MessageListener<TieAdapterConfig, TieAdapterPartitionConfig>>();
//            }

//            if (configOptions.TestEnableTestFileMessageListener)
//            {
//                services.AddTransient<IMessageListener<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>, TestFileMessageListener<TieAdapterConfig, TieAdapterPartitionConfig>>();
//            }

//            // Override default message handler for testing purposes
//            if (configOptions.TestEnableMockTie1MessageHandler)
//            {
//                services.AddTransient<IMessageHandler<TieAdapterConfig, TieAdapterPartitionConfig, Tie1Message, Tie1Receipt>, MockTie1MessageHandler>();
//            }
//        }

//        /// <summary>
//        /// Gets appSettings from app.config as a list of KeyValue pairs.
//        /// </summary>
//        private static IEnumerable<KeyValuePair<string, string>> GetAppSettingKeyValues() =>
//            ConfigurationManager.AppSettings.AllKeys
//                .Select(key => new KeyValuePair<string, string>(key, ConfigurationManager.AppSettings[key]))
//                .ToList();
//    }
//}
