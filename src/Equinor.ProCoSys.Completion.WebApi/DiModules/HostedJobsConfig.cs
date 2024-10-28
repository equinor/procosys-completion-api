using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.HostedServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class HostedJobsConfig
{
    public static WebApplicationBuilder ConfigureHostedJobs(this WebApplicationBuilder builder, bool devOnLocalhost)
    {
        if (devOnLocalhost && builder.Configuration.GetValue<bool>("MigrateDatabase"))
        {
            builder.Services.AddHostedService<DatabaseMigrator>();

            DebugOptions.DebugEntityFrameworkInDevelopment = builder.Configuration.GetValue<bool>("DebugEntityFrameworkInDevelopment");
        }
        
        builder.Services.AddHostedService<VerifyApplicationExistsAsPerson>();
        builder.Services.AddHostedService<ConfigureRequiredLabels>();
        
        return builder;
    }
}
