using System.Text.Json.Serialization;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Telemetry;
using Equinor.ProCoSys.Common.TemplateTransforming;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Command.EventHandlers;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.Command.Validators;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.WebApi.Authentication;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.MassTransit;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class ApplicationModule
{
    public static void AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApplicationOptions>(configuration.GetSection("Application"));
        services.Configure<MainApiOptions>(configuration.GetSection("MainApi"));
        services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
        services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));
        services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
        services.Configure<SyncToPCS4Options>(configuration.GetSection("SyncToPCS4Options"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.Configure<GraphOptions>(configuration.GetSection("Graph"));

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

            x.AddConsumer<ProjectEventConsumer>()
                .Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false; //MT should not create the endpoint for us, as it already exists.
                    e.Name = "completion_project";
                    e.Temporary = false;
                });

            x.AddConsumer<PersonEventConsumer>()
                .Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false; 
                    e.Name = "completion_person";
                    e.Temporary = false;
                });

            x.UsingAzureServiceBus((context,cfg) =>
            {
                var connectionString = configuration.GetConnectionString("ServiceBus");
                cfg.Host(connectionString);

                cfg.MessageTopology.SetEntityNameFormatter(new ProCoSysKebabCaseEntityNameFormatter());
                
                cfg.ConfigureJsonSerializerOptions(opts =>
                {
                    opts.Converters.Add(new OracleGuidConverter());
                    opts.Converters.Add(new JsonStringEnumConverter());
                    return opts;
                });
                cfg.SubscriptionEndpoint("completion_project","project", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<ProjectEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false; //I didn't get this to work, I think it tried to publish to endpoint that already exists in different context or something, we're logging errors anyway.
                });
                cfg.SubscriptionEndpoint("completion_person", "person", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<PersonEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false; 
                });

                // cfg.Send<PunchItemCreatedIntegrationEvent>(topologyConfigurator =>
                // {
                //     topologyConfigurator.UseSessionIdFormatter(ctx => ctx.Message.Guid.ToString());
                // });

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
        services.AddScoped<IContentAccessChecker, ContentAccessChecker>();
        services.AddScoped<ICheckListApiService, MainApiCheckListService>();
        services.AddScoped<ICheckListCache, CheckListCache>();
        services.AddScoped<IPunchItemHelper, PunchItemHelper>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<CompletionContext>());
        services.AddScoped<IReadOnlyContext, CompletionContext>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ILocalPersonRepository, LocalPersonRepository>();
        services.AddScoped<IPunchItemRepository, PunchItemRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ILinkRepository, LinkRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<ILibraryItemRepository, LibraryItemRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ISWCRRepository, SWCRRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<ILabelEntityRepository, LabelEntityRepository>();
        services.AddScoped<IMailTemplateRepository, MailTemplateRepository>();
        services.AddScoped<Command.Links.ILinkService, Command.Links.LinkService>();
        services.AddScoped<Query.Links.ILinkService, Query.Links.LinkService>();
        services.AddScoped<Command.Comments.ICommentService, Command.Comments.CommentService>();
        services.AddScoped<Query.Comments.ICommentService, Query.Comments.CommentService>();
        services.AddScoped<Command.Attachments.IAttachmentService, Command.Attachments.AttachmentService>();
        services.AddScoped<Query.Attachments.IAttachmentService, Query.Attachments.AttachmentService>();

        services.AddScoped<IAuthenticatorOptions, AuthenticatorOptions>();

        services.AddScoped<IProjectValidator, ProjectValidator>();
        services.AddScoped<IPunchItemValidator, PunchItemValidator>();
        services.AddScoped<ILibraryItemValidator, LibraryItemValidator>();
        services.AddScoped<IWorkOrderValidator, WorkOrderValidator>();
        services.AddScoped<ISWCRValidator, SWCRValidator>();
        services.AddScoped<ILabelValidator, LabelValidator>();
        services.AddScoped<ILabelEntityValidator, LabelEntityValidator>();
        services.AddScoped<IDocumentValidator, DocumentValidator>();
        services.AddScoped<ICheckListValidator, ProCoSys4CheckListValidator>();
        services.AddScoped<IRowVersionInputValidator, RowVersionInputValidator>();
        services.AddScoped<IPatchOperationInputValidator, PatchOperationInputValidator>();
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
        services.AddScoped<IAzureBlobService, AzureBlobService>();
        services.AddScoped<ITemplateTransformer, TemplateTransformer>();
        services.AddScoped<ICompletionMailService, CompletionMailService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IDeepLinkUtility, DeepLinkUtility>();

        // Singleton - Created the first time they are requested
        services.AddSingleton<ISyncToPCS4Service, SyncToPCS4Service>();
    }
}
