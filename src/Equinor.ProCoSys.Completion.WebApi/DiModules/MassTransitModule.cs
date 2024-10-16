using System;
using System.Text.Json.Serialization;
using Azure.Core;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.WebApi.MassTransit;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class MassTransitModule
{
    public static void AddMassTransitModule(this IServiceCollection services, IConfiguration configuration, TokenCredential credential)
    {
        services.AddMassTransit(x =>
        {
        x.AddEntityFrameworkOutbox<CompletionContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            x.AddConsumer<SendEmailEventConsumer>();
            x.AddConsumer<HistoryItemCreatedEventConsumer>();
            x.AddConsumer<HistoryItemUpdatedEventConsumer>();
            x.AddConsumer<HistoryItemDeletedEventConsumer>();
            x.AddConsumer<AttachmentDeletedConsumer>();
            x.AddConsumer<AttachmentCopyEventConsumer>();

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

            x.AddConsumer<LibraryEventConsumer>()
                .Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Name = "completion_library";
                    e.Temporary = false;
                });

            x.AddConsumer<DocumentEventConsumer>()
               .Endpoint(e =>
               {
                   e.ConfigureConsumeTopology = false;
                   e.Name = "completion_document";
                   e.Temporary = false;
               });
                        
            x.AddConsumer<QueryEventConsumer>()
                .Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Name = "completion_query";
                    e.Temporary = false;
                });

            x.AddConsumer<SWCREventConsumer>()
                .Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Name = "completion_swcr";
                    e.Temporary = false;
                });

            x.AddConsumer<WorkOrderEventConsumer>()
                .Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Name = "completion_wo";
                    e.Temporary = false;
                });
            x.AddConsumer<ClassificationConsumer>()
                .Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Temporary = false;
                });

            x.UsingAzureServiceBus((context, cfg) =>
            {
                var serviceBusNamespace = configuration.GetValue<string>("ServiceBusNamespace");
                if (string.IsNullOrEmpty(serviceBusNamespace))
                {
                    throw new Exception("ServiceBusNamespace is not properly configured");
                }
                var serviceUri = new Uri(serviceBusNamespace);

                cfg.Host(serviceUri, host =>
                {
                    host.TokenCredential = credential;
                });


                cfg.MessageTopology.SetEntityNameFormatter(new ProCoSysKebabCaseEntityNameFormatter());
                
                cfg.ConfigureJsonSerializerOptions(opts =>
                {
                    opts.Converters.Add(new OracleGuidConverter());
                    opts.Converters.Add(new JsonStringEnumConverter());
                    return opts;
                });

                #region Receive from queues

                cfg.ReceiveEndpoint(QueueNames.CompletionSendEmailQueue, e =>
                {
                    e.ConfigureConsumer<SendEmailEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });

                cfg.ReceiveEndpoint(QueueNames.CompletionHistoryCreated, e =>
                {
                    e.ConfigureConsumer<HistoryItemCreatedEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint(QueueNames.CompletionHistoryUpdated, e =>
                {
                    e.ConfigureConsumer<HistoryItemUpdatedEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint(QueueNames.CompletionHistoryDeleted, e =>
                {
                    e.ConfigureConsumer<HistoryItemDeletedEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint(QueueNames.LibraryCompletionTransferQueue, e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<LibraryEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint(QueueNames.SwcrCompletionTransferQueue, e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<SWCREventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint(QueueNames.DocumentCompletionTransferQueue, e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<DocumentEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                    e.PrefetchCount = configuration.GetValue<int>("MassTransit:DocumentPrefetchCount");
                });
                cfg.ReceiveEndpoint(QueueNames.WorkOrderCompletionTransferQueue, e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<WorkOrderEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint(QueueNames.ProjectCompletionTransferQueue, e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<ProjectEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint(QueueNames.PersonCompletionTransferQueue, e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<PersonEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint(QueueNames.ClassificationCompletionTransferQueue, e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<ClassificationConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                #endregion

                #region Receive from topics
                cfg.SubscriptionEndpoint("completion_project", "project", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<ProjectEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConcurrentMessageLimit = 1; //This forces consumer to handle messages one by one.
                });
                cfg.SubscriptionEndpoint("completion_person", "person", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<PersonEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConcurrentMessageLimit = 1; //This forces consumer to handle messages one by one.
                });
                cfg.SubscriptionEndpoint("completion_library", "library", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<LibraryEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConcurrentMessageLimit = 1; //This forces consumer to handle messages one by one.
                });
                cfg.SubscriptionEndpoint("completion_document", "document", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<DocumentEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConcurrentMessageLimit = 1; //This forces consumer to handle messages one by one.
                });
                cfg.SubscriptionEndpoint("completion_query", "query", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<QueryEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConcurrentMessageLimit = 1; //This forces consumer to handle messages one by one.
                });
                cfg.SubscriptionEndpoint("completion_swcr", "swcr", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<SWCREventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConcurrentMessageLimit = 1; //This forces consumer to handle messages one by one.
                });
                cfg.SubscriptionEndpoint("completion_wo", "wo", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<WorkOrderEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConcurrentMessageLimit = 1; //This forces consumer to handle messages one by one.
                });
                cfg.SubscriptionEndpoint("completion_punchprioritylibrarylation", "punchprioritylibraryrelation", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<ClassificationConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConcurrentMessageLimit = 1; //This forces consumer to handle messages one by one.
                });
                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("completion"));
                #endregion

                cfg.AutoStart = true;
            });
        });

    }
}
