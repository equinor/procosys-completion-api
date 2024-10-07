using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;

public class DocumentConsumerService(
    ILogger<DocumentConsumerService> logger,
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork) : IDocumentConsumerService 
{ 
    public async Task ConsumeDocumentEvent(ConsumeContext context, DocumentEvent busEvent, string type)
    {
        ValidateMessage(busEvent);

        var handledAs = string.Empty;
        if (busEvent.Behavior == "delete")
        {
            if (!await documentRepository.RemoveByGuidAsync(busEvent.ProCoSysGuid, context.CancellationToken))
            {
                logger.LogWarning("Document with Guid {Guid} was not found and could not be deleted. Type: {Type}",
                    busEvent.ProCoSysGuid, type);
            }

            handledAs = "delete";
        }
   
        else if (await documentRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            var document = await documentRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
            if (document.ProCoSys4LastUpdated == busEvent.LastUpdated)
            {
                logger.LogDebug("{EventName} Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {SyncedTimeStamp} \n" +
                                      "Type: {Type} \n",
                    nameof(DocumentEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated,
                    document.SyncTimestamp, type);
                return;
            }

            if (document.ProCoSys4LastUpdated > busEvent.LastUpdated)
            {
                logger.LogWarning("{EventName} Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {EventLastUpdated} \n" +
                                  "LastUpdatedFromDb: {LastUpdated} \n" +
                                  "Type: {Type} \n",
                    nameof(DocumentEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated,
                    document.ProCoSys4LastUpdated, type);
                return;
            }

            MapFromEventToDocument(busEvent, document);
            document.SyncTimestamp = DateTime.UtcNow;
            handledAs = "update";
        }
        else
        {
            var document = CreateDocumentEntity(busEvent);
            document.SyncTimestamp = DateTime.UtcNow;
            documentRepository.Add(document);
            handledAs = "create";
        }

        await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);

        logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid} \n No {No} \n Type {Type} \n HandledAs {HandledAs}",
            nameof(DocumentEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.DocumentNo, type, handledAs);
        }
    
    private static void ValidateMessage(DocumentEvent busEvent)
    {
        if (busEvent.ProCoSysGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(DocumentEvent)} is missing {nameof(DocumentEvent.ProCoSysGuid)}");
        }
        
        if(busEvent.Behavior == "delete")
        {
            return;
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
//: using fields from IDocumentEventV1;

