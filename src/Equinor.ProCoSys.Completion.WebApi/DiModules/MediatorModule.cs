using System.Reflection;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportUpdatePunchItem;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.WebApi.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class MediatorModule
{
    public static IServiceCollection AddMediatrModules(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(
            typeof(MediatorModule).GetTypeInfo().Assembly,
            typeof(ICommandMarker).GetTypeInfo().Assembly,
            typeof(IQueryMarker).GetTypeInfo().Assembly
        ));
        services.AddTransient<IRequestHandler<CreatePunchItemCommand, Result<GuidAndRowVersion>>, CreatePunchItemCommandHandler<CreatePunchItemCommand>>();
        services.AddTransient<IRequestHandler<CreatePunchItemCommandForImport, Result<GuidAndRowVersion>>, CreatePunchItemCommandHandler<CreatePunchItemCommandForImport>>();

        // ordering is important
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        // HACK validation of request need to become before checking access
        // if checking access before validation, access check will fail with Exception when
        // checking access on items which don't exist
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PrefetchBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CheckAccessBehavior<,>));
        return services;
    }
}
