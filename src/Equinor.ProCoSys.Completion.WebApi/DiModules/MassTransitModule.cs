using System.Text.Json.Serialization;
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
    public static void AddMassTransitModule(this IServiceCollection services, IConfiguration configuration)
    {
         services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<CompletionContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            x.AddConsumer<HistoryItemCreatedEventConsumer>();
            x.AddConsumer<HistoryItemUpdatedEventConsumer>();
            x.AddConsumer<HistoryItemDeletedEventConsumer>();

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
            x.AddConsumer<PunchItemEventConsumer>()
                .Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Name = "completion_punchitem";
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
                    opts.Converters.Add(new CustomDateTimeConverter());
                    return opts;
                });

                #region Receive from queues

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

                cfg.ReceiveEndpoint("libraryCompletionTransferQueue", e =>
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
                cfg.ReceiveEndpoint("swcrCompletionTransferQueue", e =>
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
                cfg.ReceiveEndpoint("documentCompletionTransferQueue", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<DocumentEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint("workOrderCompletionTransferQueue", e =>
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
                cfg.ReceiveEndpoint("punchItemCompletionTransferQueue", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<PunchItemEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                    e.ConfigureDeadLetterQueueDeadLetterTransport();
                    e.ConfigureDeadLetterQueueErrorTransport();
                });
                cfg.ReceiveEndpoint("projectCompletionTransferQueue", e =>
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
                #endregion

                #region Receive from topics
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
                cfg.SubscriptionEndpoint("completion_library", "library", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<LibraryEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                });
                cfg.SubscriptionEndpoint("completion_document", "document", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<DocumentEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                });
                cfg.SubscriptionEndpoint("completion_swcr", "swcr", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<SWCREventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                });
                cfg.SubscriptionEndpoint("completion_wo", "wo", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<WorkOrderEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                });
                cfg.SubscriptionEndpoint("completion_punchitem", "punchlistitem", e =>
                {
                    e.ClearSerialization();
                    e.UseRawJsonSerializer();
                    e.UseRawJsonDeserializer();
                    e.ConfigureConsumer<PunchItemEventConsumer>(context);
                    e.ConfigureConsumeTopology = false;
                    e.PublishFaults = false;
                });
                #endregion

                cfg.AutoStart = true;
            });
        });
        
    }
}