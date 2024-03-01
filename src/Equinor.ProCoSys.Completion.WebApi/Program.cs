using System;
using Azure.Identity;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

namespace Equinor.ProCoSys.Completion.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        host.Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                if (context.HostingEnvironment.IsIntegrationTest())
                {
                    return;
                }

                // will it find this?
                var apiSecret = "uds983jna92oijfprgyuh2309";
                var settings = config.Build();
                var azConfig = settings.GetValue<bool>("UseAzureAppConfiguration");
                if (azConfig)
                {
                    config.AddAzureAppConfiguration(options =>
                    {
                        var connectionString = settings["ConnectionStrings:AppConfig"] + apiSecret;
                        options.Connect(connectionString)
                            .ConfigureKeyVault(kv =>
                            {
                                kv.SetCredential(new ManagedIdentityCredential());
                            })
                            .Select(KeyFilter.Any)
                            .Select(KeyFilter.Any, context.HostingEnvironment.EnvironmentName)
                            .ConfigureRefresh(refreshOptions =>
                            {
                                refreshOptions.Register("Sentinel", true);
                                refreshOptions.SetCacheExpiration(TimeSpan.FromSeconds(30));
                            });
                    });
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    options.AddServerHeader = false;
                    options.Limits.MaxRequestBodySize = null;
                });
                webBuilder.UseStartup<Startup>();
            });
}
