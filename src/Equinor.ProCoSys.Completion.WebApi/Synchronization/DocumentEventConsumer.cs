using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class DocumentEventConsumer(
    ILogger<DocumentEventConsumer> logger,
    IPlantSetter plantSetter,
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserSetter currentUserSetter,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : IConsumer<DocumentEvent>
{
    public async Task Consume(ConsumeContext<DocumentEvent> context)
    {
        var busEvent = context.Message;

        ValidateMessage(busEvent);
        plantSetter.SetPlant(busEvent.Plant);

        if (await documentRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var document = await documentRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            if (document.ProCoSys4LastUpdated == busEvent.LastUpdated)
            {
                logger.LogInformation("Document Message Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {SyncedTimeStamp} \n",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, document.SyncedTimeStamp );
                return;
            }

            if (document.ProCoSys4LastUpdated > busEvent.LastUpdated)
            {
                logger.LogWarning("Document Message Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {EventLastUpdated} \n" +
                                  "LastUpdatedFromDb: {LastUpdated}",
                    context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated, document.ProCoSys4LastUpdated);
                return;
            }
            MapFromEventToDocument(busEvent, document);
            document.SyncedTimeStamp = DateTime.UtcNow;
        }
        else
        {
            var document = CreateDocumentEntity(busEvent);
            document.SyncedTimeStamp = DateTime.UtcNow;
            documentRepository.Add(document);
        }
        
        currentUserSetter.SetCurrentUserOid(applicationOptions.CurrentValue.ObjectId);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation($"{nameof(DocumentEvent)} Message Consumed: {{MessageId}} \n Guid {{Guid}} \n No {{No}}",
            context.MessageId, busEvent.ProCoSysGuid, busEvent.DocumentNo);
    }

    private static void ValidateMessage(DocumentEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(DocumentEvent)} is missing {nameof(DocumentEvent.ProCoSysGuid)}");
        }

        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception($"{nameof(DocumentEvent)} is missing {nameof(DocumentEvent.Plant)}");
        }
    }

    private static Document CreateDocumentEntity(DocumentEvent busEvent)
    {
        var document = new Document(
            busEvent.Plant,
            busEvent.ProCoSysGuid,
            busEvent.DocumentNo
        ) { ProCoSys4LastUpdated = busEvent.LastUpdated, IsVoided = busEvent.IsVoided};
        return document;
    }
    
    private static void MapFromEventToDocument(DocumentEvent busEvent, Document document)
    {
        document.No = busEvent.DocumentNo;
        document.ProCoSys4LastUpdated = busEvent.LastUpdated;
        document.IsVoided = busEvent.IsVoided;
    }
}

public record DocumentEvent(
    string Plant,
    Guid ProCoSysGuid,
    string DocumentNo,
    bool IsVoided,
    DateTime LastUpdated
); //: using fields from IDocumentEventV1;
