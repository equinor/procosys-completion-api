using Equinor.ProCoSys.Completion.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using System.Threading;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PunchItemAttachmentEventConsumer(
    ILogger<PunchItemAttachmentEventConsumer> logger,
    IPersonRepository personRepository,
    IAttachmentRepository attachmentRepository,
    ILinkRepository linkRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<PunchItemAttachmentEvent>
{
    public async Task Consume(ConsumeContext<PunchItemAttachmentEvent> context)
    {
        var busEvent = context.Message;
        ValidateAttachmentMessage(busEvent);

        if (busEvent.Behavior == "delete")
        {
            if (!await attachmentRepository.RemoveByGuidAsync(busEvent.AttachmentGuid, context.CancellationToken)
                && !await linkRepository.RemoveByGuidAsync(busEvent.AttachmentGuid, context.CancellationToken))
            {
                logger.LogWarning("Attachment with Guid {Guid} was not found and could not be deleted",
                    busEvent.AttachmentGuid);
            }
        }
        else if (EventIsAttachment(busEvent))
        {
            ValidateAttachmentMessageAsCompletionAttachment(busEvent);
            if (!await HandleAttachmentEventAsync(context, busEvent))
            {
                return;
            }
        }
        else
        {
            ValidateAttachmentMessageAsCompletionLink(busEvent);
            if (!await HandleLinkEvent(context, busEvent))
            {
                return;
            }
        }

        await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);

        logger.LogDebug("{EventName} Message Consumed: {MessageId} \n Guid {Guid} \n Title {Title}",
            nameof(PunchItemAttachmentEvent), context.MessageId, busEvent.AttachmentGuid, busEvent.Title);
    }

    private async Task<bool> HandleAttachmentEventAsync(ConsumeContext<PunchItemAttachmentEvent> context, PunchItemAttachmentEvent busEvent)
    {
        if (await attachmentRepository.ExistsAsync(busEvent.AttachmentGuid, context.CancellationToken))
        {
            var attachment = await attachmentRepository.GetAsync(busEvent.AttachmentGuid, context.CancellationToken);

            if (attachment.ProCoSys4LastUpdated == busEvent.LastUpdated)
            {
                logger.LogDebug("{EventName} Message Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {SyncedTimeStamp} \n",
                    nameof(PunchItemAttachmentEvent), context.MessageId, busEvent.AttachmentGuid, busEvent.LastUpdated,
                    attachment.SyncTimestamp);
                return false;
            }

            if (attachment.ProCoSys4LastUpdated > busEvent.LastUpdated)
            {
                logger.LogWarning("{EventName} Message Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {EventLastUpdated} \n" +
                                  "LastUpdatedFromDb: {LastUpdated}",
                    nameof(PunchItemAttachmentEvent), context.MessageId, busEvent.AttachmentGuid, busEvent.LastUpdated,
                    attachment.ProCoSys4LastUpdated);
                return false;
            }

            MapFromEventToAttachment(busEvent, attachment);
        }
        else
        {
            var attachment = await CreateAttachmentEntityAsync(busEvent, context.CancellationToken);
            attachmentRepository.Add(attachment);
        }

        return true;
    }

    private async Task<bool> HandleLinkEvent(ConsumeContext<PunchItemAttachmentEvent> context, PunchItemAttachmentEvent busEvent)
    {
        if (await linkRepository.ExistsAsync(busEvent.AttachmentGuid, context.CancellationToken))
        {
            var attachment = await linkRepository.GetAsync(busEvent.AttachmentGuid, context.CancellationToken);

            if (attachment.ProCoSys4LastUpdated == busEvent.LastUpdated)
            {
                logger.LogInformation("{EventName} Message Ignored because LastUpdated is the same as in db\n" +
                                      "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                      "EventLastUpdated: {LastUpdated} \n" +
                                      "SyncedToCompletion: {SyncedTimeStamp} \n",
                    nameof(PunchItemAttachmentEvent), context.MessageId, busEvent.AttachmentGuid, busEvent.LastUpdated,
                    attachment.SyncTimestamp);
                return false;
            }

            if (attachment.ProCoSys4LastUpdated > busEvent.LastUpdated)
            {
                logger.LogWarning("{EventName} Message Ignored because a newer LastUpdated already exits in db\n" +
                                  "MessageId: {MessageId} \n ProCoSysGuid: {ProCoSysGuid} \n " +
                                  "EventLastUpdated: {EventLastUpdated} \n" +
                                  "LastUpdatedFromDb: {LastUpdated}",
                    nameof(PunchItemAttachmentEvent), context.MessageId, busEvent.AttachmentGuid, busEvent.LastUpdated,
                    attachment.ProCoSys4LastUpdated);
                return false;
            }

            MapFromEventToLink(busEvent, attachment);
        }
        else
        {
            var attachment = await CreateLinkEntityAsync(busEvent, context.CancellationToken);
            linkRepository.Add(attachment);
        }

        return true;
    }

    private bool EventIsAttachment(PunchItemAttachmentEvent busEvent)
        => busEvent.FileId is > 0 && !string.IsNullOrEmpty(busEvent.FileName);

    private void ValidateAttachmentMessage(PunchItemAttachmentEvent busEvent)
    {
        if (busEvent.AttachmentGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(PunchItemAttachmentEvent)} is missing {nameof(PunchItemAttachmentEvent.AttachmentGuid)}");
        }
        if (string.IsNullOrEmpty(busEvent.Plant))
        {
            throw new Exception($"{nameof(PunchItemAttachmentEvent)} is missing {nameof(PunchItemAttachmentEvent.Plant)}");
        }
        if (busEvent.PunchItemGuid == Guid.Empty && busEvent.Behavior != "delete")
        {
            throw new Exception($"{nameof(PunchItemAttachmentEvent)} is missing {nameof(PunchItemAttachmentEvent.PunchItemGuid)}");
        }
    }

    private void ValidateAttachmentMessageAsCompletionAttachment(PunchItemAttachmentEvent busEvent)
    {
        if (string.IsNullOrEmpty(busEvent.ProjectName))
        {
            throw new Exception($"{nameof(PunchItemAttachmentEvent)} is missing {nameof(PunchItemAttachmentEvent.ProjectName)}");
        }
        if (busEvent.FileId is > 0 && string.IsNullOrEmpty(busEvent.FileName))
        {
            throw new Exception($"{nameof(PunchItemAttachmentEvent)} has {nameof(PunchItemAttachmentEvent.FileId)}, but is missing {nameof(PunchItemAttachmentEvent.FileName)}");
        }
    }

    private void ValidateAttachmentMessageAsCompletionLink(PunchItemAttachmentEvent busEvent)
    {
        if (string.IsNullOrEmpty(busEvent.Title))
        {
            throw new Exception($"{nameof(PunchItemAttachmentEvent)} is missing {nameof(PunchItemAttachmentEvent.Title)}");
        }
        if (string.IsNullOrEmpty(busEvent.Uri))
        {
            throw new Exception($"{nameof(PunchItemAttachmentEvent)} is missing {nameof(PunchItemAttachmentEvent.Uri)}");
        }
    }

    private void MapFromEventToAttachment(PunchItemAttachmentEvent busEvent, Attachment attachment)
    {
        attachment.ProCoSys4LastUpdated = busEvent.LastUpdated;
        attachment.ProCoSys4LastUpdatedByUser = busEvent.LastUpdatedByUser;
        attachment.SyncTimestamp = DateTime.UtcNow;
        attachment.Description = busEvent.Title;
        attachment.SetSyncProperties(busEvent.LastUpdated);
    }

    private async Task<Attachment> CreateAttachmentEntityAsync(PunchItemAttachmentEvent busEvent, CancellationToken cancellationToken)
    {
        var attachment = new Attachment(
            busEvent.ProjectName, 
            nameof(PunchItem), 
            busEvent.PunchItemGuid, 
            busEvent.FileName!, 
            busEvent.AttachmentGuid)
        {
            ProCoSys4LastUpdated = busEvent.LastUpdated,
            ProCoSys4LastUpdatedByUser = busEvent.LastUpdatedByUser, 
            SyncTimestamp = DateTime.UtcNow
        };
        if (!string.IsNullOrEmpty(busEvent.Title))
        {
            attachment.Description = busEvent.Title;
        }

        var person = await personRepository.GetAsync(busEvent.CreatedByGuid, cancellationToken);
        attachment.SetSyncProperties(person, busEvent.CreatedAt, busEvent.LastUpdated);

        return attachment;
    }

    private void MapFromEventToLink(PunchItemAttachmentEvent busEvent, Link link)
    {
        link.ProCoSys4LastUpdated = busEvent.LastUpdated;
        link.ProCoSys4LastUpdatedByUser = busEvent.LastUpdatedByUser;
        link.SyncTimestamp = DateTime.UtcNow;
        link.Title = busEvent.Title;
        link.Url = busEvent.Uri!;
        link.SetSyncProperties(busEvent.LastUpdated);
    }

    private async Task<Link> CreateLinkEntityAsync(PunchItemAttachmentEvent busEvent, CancellationToken cancellationToken)
    {
        var link = new Link(nameof(PunchItem), busEvent.PunchItemGuid, busEvent.Title, busEvent.Uri!, busEvent.AttachmentGuid)
        {
            ProCoSys4LastUpdated = busEvent.LastUpdated,
            ProCoSys4LastUpdatedByUser = busEvent.LastUpdatedByUser,
            SyncTimestamp = DateTime.UtcNow
        };

        var person = await personRepository.GetAsync(busEvent.CreatedByGuid, cancellationToken);
        link.SetSyncProperties(person, busEvent.CreatedAt, busEvent.LastUpdated);

        return link;
    }
}

// In ProCoSys4, attachments (blobs) and links are handled as an Attachment entity
// EventIsAttachment distinguish between such
public record PunchItemAttachmentEvent
(
    string Plant,
    string ProjectName,
    Guid PunchItemGuid,
    Guid AttachmentGuid,
    string? FileName,
    string? Uri,
    string Title,
    int? FileId,
    Guid CreatedByGuid,
    DateTime CreatedAt,
    DateTime LastUpdated,
    string? LastUpdatedByUser,
    string? Behavior
);
