﻿using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.TemplateTransforming;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Command.EventHandlers;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Command.Validators;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.FormularTypes;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.Responsibles;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.TagFunctions;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Equinor.ProCoSys.Completion.Query.UserDelegationProvider;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class ApplicationModule
{
    private const string GraphSection = "Graph";

    public static async Task AddApplicationModules(this IServiceCollection services, IConfiguration configuration, TokenCredential credential)
    {
        services.Configure<ApplicationOptions>(configuration.GetSection("Application"));
        services.Configure<MainApiOptions>(configuration.GetSection("MainApi"));
        services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
        services.Configure<MainApiAuthenticatorOptions>(configuration.GetSection("AzureAd"));
        services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
        services.Configure<SyncToPCS4Options>(configuration.GetSection("SyncToPCS4Options"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));

        var graphClientId = configuration[$"{GraphSection}:ClientId"];
        if (graphClientId == null)
        {
            throw new InvalidOperationException("Graph:ClientId configuration is missing");
        }

        var mailCredential = configuration.IsDevOnLocalhost()
            ? await CertificateCredentialProvider.GetClientCertificateTokenCredential(configuration, GraphSection, credential)
            : new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                WorkloadIdentityClientId = graphClientId
            });
        
        services.AddKeyedSingleton("mailCredential", mailCredential);

        services.AddDbContext<CompletionContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(CompletionContext.CompletionContextConnectionStringName);
            options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });

        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.AddApplicationInsights();
        });

        services.AddMassTransitModule(configuration, credential);

        services.AddHttpContextAccessor();
        services.AddHttpClient();

        // Hosted services

        // Singleton - Created the first time they are requested
        services.AddSingleton<IUserDelegationProvider, UserDelegationProvider>();

        // Scoped - Created once per client request (connection)
        services.AddScoped<IAccessValidator, AccessValidator>();
        services.AddScoped<IProjectAccessChecker, ProjectAccessChecker>();
        services.AddScoped<IAccessChecker, AccessChecker>();
        services.AddScoped<ICheckListApiService, MainApiCheckListService>();
        services.AddScoped<IResponsibleApiService, MainApiResponsibleService>();
        services.AddScoped<ITagFunctionApiService, MainApiTagFunctionService>();
        services.AddScoped<IFormularTypeApiService, MainApiFormularTypeService>();
        services.AddScoped<ICheckListCache, CheckListCache>();
        services.AddScoped<IPersonApiService, MainApiPersonService>();
        services.AddScoped<IPersonCache, PersonCache>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<CompletionContext>());
        services.AddScoped<IReadOnlyContext, CompletionContext>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ILocalPersonRepository, LocalPersonRepository>();
        services.AddScoped<IPunchItemRepository, PunchItemRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<ILibraryItemRepository, LibraryItemRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ISWCRRepository, SWCRRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<ILabelEntityRepository, LabelEntityRepository>();
        services.AddScoped<IMailTemplateRepository, MailTemplateRepository>();
        services.AddScoped<IHistoryItemRepository, HistoryItemRepository>();
        services.AddScoped<Command.Comments.ICommentService, Command.Comments.CommentService>();
        services.AddScoped<Query.Comments.ICommentService, Query.Comments.CommentService>();
        services.AddScoped<Query.History.IHistoryService, Query.History.HistoryService>();
        services.AddScoped<Command.Attachments.IAttachmentService, Command.Attachments.AttachmentService>();
        services.AddScoped<Query.Attachments.IAttachmentService, Query.Attachments.AttachmentService>();
        services.AddScoped<Command.ModifiedEvents.IModifiedEventService, Command.ModifiedEvents.ModifiedEventService>();
        services.AddScoped<IPunchItemService, PunchItemService>();
        services.AddScoped<ICacheManager, DistributedCacheManager>();
        
        services.AddScoped<IProjectValidator, ProjectValidator>();
        services.AddScoped<IPunchItemValidator, PunchItemValidator>();
        services.AddScoped<ILibraryItemValidator, LibraryItemValidator>();
        services.AddScoped<IWorkOrderValidator, WorkOrderValidator>();
        services.AddScoped<ISWCRValidator, SWCRValidator>();
        services.AddScoped<ILabelValidator, LabelValidator>();
        services.AddScoped<ILabelEntityValidator, LabelEntityValidator>();
        services.AddScoped<IDocumentValidator, DocumentValidator>();
        services.AddScoped<IRowVersionInputValidator, RowVersionInputValidator>();
        services.AddScoped<IPatchOperationInputValidator, PatchOperationInputValidator>();
        services.AddScoped<IMessageProducer, MessageProducer>();
        services.AddScoped<IAzureBlobService, AzureBlobService>();
        services.AddScoped<ITemplateTransformer, TemplateTransformer>();
        services.AddScoped<ICompletionMailService, CompletionMailService>();

        services.AddTransient<IEmailService, EmailService>(provider => 
            new EmailService(provider.GetRequiredService<IOptionsMonitor<EmailOptions>>(),
                provider.GetRequiredKeyedService<TokenCredential>("mailCredential"), 
            provider.GetRequiredService<ILogger<EmailService>>()));

        services.AddScoped<IDeepLinkUtility, DeepLinkUtility>();
        services.AddScoped<IUserPropertyHelper, UserPropertyHelper>();
        services.AddScoped<IDocumentConsumerService, DocumentConsumerService>();

        services.AddScoped<ISyncToPCS4Service, SyncToPCS4Service>();
        services.AddScoped<ISyncTokenService, SyncTokenService>();

        // Transient - Created each time it is requested from the service container
        services.AddTransient<SyncBearerTokenHandler>();

        // HttpClient - Creates a specifically configured HttpClient
        services.AddHttpClient(SyncToPCS4Service.ClientName)
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptionsMonitor<SyncToPCS4Options>>().CurrentValue;
                client.BaseAddress = new Uri(options.Endpoint);
            })
            .AddHttpMessageHandler<SyncBearerTokenHandler>();
    }
}
