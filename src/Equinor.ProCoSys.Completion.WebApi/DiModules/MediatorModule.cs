using System.Reflection;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.WebApi.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class MediatorModule
{
    public static void AddMediatrModules(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(
            typeof(MediatorModule).GetTypeInfo().Assembly,
            typeof(ICommandMarker).GetTypeInfo().Assembly,
            typeof(IQueryMarker).GetTypeInfo().Assembly
        ));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CheckValidProjectBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CheckAccessBehavior<,>));
    }
}