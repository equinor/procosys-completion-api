using System;
using Azure.Identity;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class SetupAppConfig
{
    public static WebApplicationBuilder ConfigureAppConfig(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsIntegrationTest())
        {
            var azConfig = builder.Configuration.GetValue<bool>("UseAzureAppConfiguration");
            if (azConfig)
            {
                builder.Configuration.AddAzureAppConfiguration(options =>
                {
                    var connectionString = builder.Configuration["ConnectionStrings:AppConfig"];
                    options.Connect(connectionString)
                        .ConfigureKeyVault(kv =>
                        {
                            if (builder.Configuration.IsDevOnLocalhost())
                            {
                                kv.SetCredential(new DefaultAzureCredential());
                            }
                            else
                            {
                                kv.SetCredential(new ManagedIdentityCredential());
                            }
                        })
                        .Select(KeyFilter.Any)
                        .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
                        .ConfigureRefresh(refreshOptions =>
                        {
                            refreshOptions.Register("Sentinel", true);
                            refreshOptions.SetCacheExpiration(TimeSpan.FromSeconds(30));
                        });
                });
            }
        }
        
        return builder;
    } 
}
