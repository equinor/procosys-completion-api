using System.Configuration;
using Azure.Core;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class TelemetryConfig
{
    public static WebApplicationBuilder ConfigureTelemetry(this WebApplicationBuilder builder, TokenCredential credential, bool devOnLocalhost)
    {
        builder.Services.AddSingleton<TelemetryConfiguration>(sp =>
        {
            var config = TelemetryConfiguration.CreateDefault();
            config.ConnectionString = builder.Configuration.GetRequiredConfiguration("ApplicationInsights:ConnectionString");
            config.SetAzureTokenCredential(sp.GetRequiredService<TokenCredential>());
            return config;
        });

        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = builder.Configuration.GetRequiredConfiguration("ApplicationInsights:ConnectionString");
        });
        builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
        {
            module.EnableSqlCommandTextInstrumentation = builder.Configuration.GetValue("EnableSqlCommandTextInstrumentation", false);
        });
        
        return builder;
    }
}
