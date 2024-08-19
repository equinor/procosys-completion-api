using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.HostedServices;
using Equinor.ProCoSys.Completion.WebApi.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        if (!builder.Environment.IsProduction() && !builder.Environment.IsTest() && builder.Configuration.GetValue<bool>("SeedDummyData"))
        {
            builder.Services.AddHostedService<Seeder>();
        }
        
        builder.Services.AddHostedService<VerifyApplicationExistsAsPerson>();
        // VerifyLabelEntitiesExists need to come after VerifyApplicationExistsAsPerson!
        builder.Services.AddHostedService<ConfigureRequiredLabels>();
        
        return builder;
    }
}
