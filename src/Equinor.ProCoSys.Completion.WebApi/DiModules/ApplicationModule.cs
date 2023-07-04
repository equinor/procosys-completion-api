using Equinor.ProCoSys.Completion.Command.EventHandlers;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.WebApi.Authentication;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Telemetry;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.WebApi.MassTransit;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class ApplicationModule
{
    public static void AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApplicationOptions>(configuration.GetSection("Application"));
        services.Configure<MainApiOptions>(configuration.GetSection("MainApi"));
        services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
        services.Configure<CompletionAuthenticatorOptions>(configuration.GetSection("Authenticator"));
        services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));

        services.AddDbContext<CompletionContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(CompletionContext.CompletionContextConnectionStringName);
            options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });

        services.AddLogging(configure => configure.AddConsole());

        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<CompletionContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });
            
            x.UsingAzureServiceBus((context,cfg) =>
            {
                var connectionString = configuration.GetConnectionString("ServiceBus");
                cfg.Host(connectionString);
                
                cfg.MessageTopology.SetEntityNameFormatter(new ProCoSysKebabCaseEntityNameFormatter());
                
                
                cfg.Send<PunchCreatedIntegrationEvent>(topologyConfigurator =>
                {
                    topologyConfigurator.UseSessionIdFormatter(ctx => ctx.Message.Guid.ToString());
                });
                
                cfg.AutoStart = true;
            });
        });

        services.AddHttpContextAccessor();
        services.AddHttpClient();

        // Hosted services

        // Transient - Created each time it is requested from the service container

        // Scoped - Created once per client request (connection)
        services.AddScoped<ITelemetryClient, ApplicationInsightsTelemetryClient>();
        services.AddScoped<IAccessValidator, AccessValidator>();
        services.AddScoped<IProjectAccessChecker, ProjectAccessChecker>();
        services.AddScoped<IProjectChecker, ProjectChecker>();
        services.AddScoped<IPunchHelper, PunchHelper>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<CompletionContext>());
        services.AddScoped<IReadOnlyContext, CompletionContext>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ILocalPersonRepository, LocalPersonRepository>();
        services.AddScoped<IPunchRepository, PunchRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ILinkRepository, LinkRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<Command.Links.ILinkService, Command.Links.LinkService>();
        services.AddScoped<Query.Links.ILinkService, Query.Links.LinkService>();
        services.AddScoped<Command.Comments.ICommentService, Command.Comments.CommentService>();
        services.AddScoped<Query.Comments.ICommentService, Query.Comments.CommentService>();
        services.AddScoped<Command.Attachments.IAttachmentService, Command.Attachments.AttachmentService>();
        services.AddScoped<Query.Attachments.IAttachmentService, Query.Attachments.AttachmentService>();

        services.AddScoped<IAuthenticatorOptions, AuthenticatorOptions>();

        services.AddScoped<IProjectValidator, ProjectValidator>();
        services.AddScoped<IPunchValidator, PunchValidator>();
        services.AddScoped<IRowVersionValidator, RowVersionValidator>();

        services.AddScoped<IAzureBlobService, AzureBlobService>();

        // Singleton - Created the first time they are requested
        services.AddSingleton<IEmailService, EmailService>();
    }
}
