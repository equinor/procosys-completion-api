using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization.Services;

public class DocumentConsumerService(
    ILogger<DocumentConsumerService> logger,
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork) : IDocumentConsumerService
{
    private const string DuplicateKeyError = "Cannot insert duplicate key row in object 'dbo.Documents' with unique index 'IX_Documents_Guid'";
    private const int MaxRetryCount = 2;

    public async Task ConsumeDocumentEvent(ConsumeContext context, DocumentEvent busEvent, string type)
    {
        ValidateMessage(busEvent);

        // To avoid cases of dead letters caused by a race condition
        // we retry consuming the document event a couple of times before we give up
        var retryCount = 0;
        while (retryCount < MaxRetryCount)
        {
            try
            {
                var handledAs = string.Empty;
                if (busEvent.Behavior == "delete")
                {
                    await HandleDocumentDeleteEvent(context, busEvent, type);
                }

                else if (await documentRepository.ExistsAsync(busEvent.ProCoSysGuid, context.CancellationToken))
                {
                    var didUpdate = await HandleDocumentUpdateEvent(context, busEvent, type);
                    if (!didUpdate)
                    {
                        return;
                    }
                }
                else
                {
                    HandleDocumentCreateEvent(context, busEvent, type);
                }

                await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);

                logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid} \n No {No} \n Type {Type} \n HandledAs {HandledAs}",
                    nameof(DocumentEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.DocumentNo, type, busEvent.Behavior);

                return;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var warningMessage = "failed because of concurrency exception in the db";
                LogConcurrencyException(ex, context, busEvent, type, warningMessage);

                Thread.Sleep(200);
                retryCount++;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(DuplicateKeyError) || (ex.InnerException != null && ex.InnerException.Message.Contains(DuplicateKeyError)))
                {
                    var warningMessage = "failed because of a duplicate key row for a unique index";
                    LogConcurrencyException(ex, context, busEvent, type, warningMessage);

                    Thread.Sleep(200);
                    retryCount++;
                }
                else
                {
                    throw;
                }
            }
        }
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

    private async Task HandleDocumentDeleteEvent(ConsumeContext context, DocumentEvent busEvent, string type)
    {
        if (!await documentRepository.RemoveByGuidAsync(busEvent.ProCoSysGuid, context.CancellationToken))
        {
            logger.LogWarning("Document with Guid {Guid} was not found and could not be deleted. \n" +
                                    "MessageID {MessageId} \n" +
                                    "Type: {Type}",
                busEvent.ProCoSysGuid, context.MessageId, type);
        }
        logger.LogWarning("Document with Guid {Guid} was deleted. Type: {Type}", busEvent.ProCoSysGuid, type);
    }

    private async Task<bool> HandleDocumentUpdateEvent(ConsumeContext context, DocumentEvent busEvent, string type)
    {
        var document = await documentRepository.GetAsync(busEvent.ProCoSysGuid, context.CancellationToken);
        if (document.ProCoSys4LastUpdated == busEvent.LastUpdated)
        {
            logger.LogDebug("{EventName} Ignored because LastUpdated is the same as in db \n" +
                                  "MessageId: {MessageId} \n" +
                                  "ProCoSysGuid: {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {LastUpdated} \n" +
                                  "SyncedToCompletion: {SyncedTimeStamp} \n" +
                                  "Type: {Type}",
                nameof(DocumentEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated,
                document.SyncTimestamp, type);
            return false;
        }

        if (document.ProCoSys4LastUpdated > busEvent.LastUpdated)
        {
            logger.LogWarning("{EventName} Ignored because a newer LastUpdated already exits in db \n" +
                              "MessageId: {MessageId} \n" +
                              "ProCoSysGuid: {ProCoSysGuid} \n " +
                              "EventLastUpdated: {EventLastUpdated} \n" +
                              "LastUpdatedFromDb: {LastUpdated} \n" +
                              "Type: {Type}",
                nameof(DocumentEvent), context.MessageId, busEvent.ProCoSysGuid, busEvent.LastUpdated,
                document.ProCoSys4LastUpdated, type);
            return false;
        }

        MapFromEventToDocument(busEvent, document);
        document.SyncTimestamp = DateTime.UtcNow;
        return true;
    }

    private void HandleDocumentCreateEvent(ConsumeContext context, DocumentEvent busEvent, string type)
    {
        var document = CreateDocumentEntity(busEvent);
        document.SyncTimestamp = DateTime.UtcNow;
        documentRepository.Add(document);
    }

    private void LogConcurrencyException(Exception ex, ConsumeContext context, DocumentEvent busEvent, string type, string message)
    {
        logger.LogWarning(ex, "{EventName} {Message} \n" +
                          "MessageId: {MessageId} \n" +
                          "ProCoSysGuid: {ProCoSysGuid} \n" +
                          "Type: {Type}",
            nameof(DocumentEvent), context.MessageId, busEvent.ProCoSysGuid, type, message);
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

