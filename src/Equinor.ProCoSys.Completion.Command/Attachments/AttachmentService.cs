using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Command.ModifiedEvents;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Command.Attachments;

public class AttachmentService(
    IAttachmentRepository attachmentRepository,
    IPlantProvider plantProvider,
    IUnitOfWork unitOfWork,
    IAzureBlobService azureBlobService,
    IOptionsSnapshot<BlobStorageOptions> blobStorageOptions,
    IMessageProducer messageProducer,
    ILogger<AttachmentService> logger,
    IModifiedEventService modifiedEventService,
    ISyncToPCS4Service syncToPCS4Service)
    : IAttachmentService
{
    public async Task<AttachmentDto> UploadNewAsync(
        string project,
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var attachment = await attachmentRepository.GetAttachmentWithFileNameForParentAsync(parentGuid, fileName, cancellationToken);

            if (attachment is not null)
            {
                throw new Exception($"{parentType} {parentGuid} already has an attachment with filename {fileName}");
            }

            attachment = new Attachment(
                project,
                parentType,
                parentGuid,
                fileName);
            attachmentRepository.Add(attachment);

            var verifiedContentType = await DetermineContentTypeAsync(content, fileName, cancellationToken);
            await UploadAsync(attachment, content, false, verifiedContentType, cancellationToken);

            // ReSharper disable once UnusedVariable
            var integrationEvent = await PublishCreatedEventsAsync(attachment, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await syncToPCS4Service.SyncNewAttachmentAsync(integrationEvent, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return new AttachmentDto(attachment.Guid, attachment.RowVersion.ConvertToString());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred on insertion of Attachment");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<string> UploadOverwriteAsync(
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        string contentType,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var attachment = await attachmentRepository.GetAttachmentWithFileNameForParentAsync(parentGuid, fileName, cancellationToken);

            if (attachment is null)
            {
                throw new Exception($"{parentType} {parentGuid} don't have an attachment with filename {fileName}");
            }

            var changes = SetRevisionNumber(attachment);

            var verifiedContentType = await DetermineContentTypeAsync(content, fileName, cancellationToken);
            await UploadAsync(attachment, content, true, verifiedContentType, cancellationToken);

            // ReSharper disable once UnusedVariable
            var integrationEvent = await PublishUpdatedEventsAsync(
                $"Attachment {attachment.FileName} uploaded again",
                attachment,
                changes,
                cancellationToken);

            attachment.SetRowVersion(rowVersion);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await syncToPCS4Service.SyncAttachmentUpdateAsync(integrationEvent, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return attachment.RowVersion.ConvertToString();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred on update of Attachment with parent guid {parentGuid}", parentGuid);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> FileNameExistsForParentAsync(Guid parentGuid, string fileName, CancellationToken cancellationToken)
    {
        var attachment = await attachmentRepository.GetAttachmentWithFileNameForParentAsync(parentGuid, fileName, cancellationToken);
        return attachment is not null;
    }

    public async Task DeleteAsync(
        Guid guid,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var attachment = await attachmentRepository.GetAsync(guid, cancellationToken);
            // ReSharper disable once UnusedVariable
            var integrationEvent = await PublishDeletedEventsAsync(attachment, cancellationToken);

            // Set correct Concurrency
            attachment.SetRowVersion(rowVersion);
            attachmentRepository.Remove(attachment);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await syncToPCS4Service.SyncAttachmentDeleteAsync(integrationEvent, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Attachment '{AttachmentFileName}' with guid {AttachmentGuid} deleted for {AttachmentParentGuid}",
                attachment.FileName,
                attachment.Guid,
                attachment.ParentGuid);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred on deletion of Attachment with guid {guid}", guid);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid guid,
        CancellationToken cancellationToken)
        => await attachmentRepository.ExistsAsync(guid, cancellationToken);

    public async Task<string> UpdateAsync(
        Guid guid,
        string description,
        IEnumerable<Label> labels,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var attachment = await attachmentRepository.GetAttachmentWithLabelsAsync(guid, cancellationToken);
            attachment.UpdateLabels(labels.ToList());

            var changes = UpdateAttachment(attachment, description);
            // ReSharper disable once NotAccessedVariable
            AttachmentUpdatedIntegrationEvent? integrationEvent = null;
            if (changes.Any())
            {
                // ReSharper disable once RedundantAssignment
                integrationEvent = await PublishUpdatedEventsAsync($"Attachment {attachment.FileName} updated", attachment, changes, cancellationToken);
            }
            attachment.SetRowVersion(rowVersion);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (integrationEvent != null)
            {
                await syncToPCS4Service.SyncAttachmentUpdateAsync(integrationEvent, cancellationToken);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return attachment.RowVersion.ConvertToString();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred on update of Attachment with guid {guid}", guid);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private List<IChangedProperty> UpdateAttachment(Attachment attachment, string description)
    {
        var changes = new List<IChangedProperty>();

        if (attachment.Description != description)
        {
            changes.Add(new ChangedProperty<string>(
                nameof(Attachment.Description),
                attachment.Description,
                description));
            attachment.Description = description;
        }

        return changes;
    }

    private async Task UploadAsync(
        Attachment attachment,
        Stream content,
        bool overwriteIfExists,
        string contentType,
        CancellationToken cancellationToken)
    {
        var fullBlobPath = attachment.GetFullBlobPath();
        await azureBlobService.UploadAsync(
            blobStorageOptions.Value.BlobContainer,
            fullBlobPath,
            content,
            contentType,
            overwriteIfExists,
            cancellationToken);

        logger.LogInformation("Attachment '{AttachmentFileName}' with guid {AttachmentGuid} uploaded for {AttachmentParentGuid}",
            attachment.FileName,
            attachment.Guid,
            attachment.ParentGuid);
    }

    private async Task<AttachmentCreatedIntegrationEvent> PublishCreatedEventsAsync(Attachment attachment, CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var integrationEvent = new AttachmentCreatedIntegrationEvent(attachment, plantProvider.Plant);
        await messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var properties = new List<IProperty>
        {
            new Property(nameof(Attachment.FileName), attachment.FileName)
        };
        var historyEvent = new HistoryCreatedIntegrationEvent(
            $"Attachment {attachment.FileName} uploaded",
            attachment.Guid,
            attachment.ParentGuid,
            new User(attachment.CreatedBy.Guid, attachment.CreatedBy.GetFullName()),
            attachment.CreatedAtUtc,
            properties);
        await messageProducer.SendHistoryAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private async Task<AttachmentUpdatedIntegrationEvent> PublishUpdatedEventsAsync(
        string displayName,
        Attachment attachment, 
        List<IChangedProperty> changes, 
        CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var integrationEvent = new AttachmentUpdatedIntegrationEvent(attachment, plantProvider.Plant);
        await messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryUpdatedIntegrationEvent(
            displayName,
            attachment.Guid,
            attachment.ParentGuid,
            new User(attachment.ModifiedBy!.Guid, attachment.ModifiedBy!.GetFullName()),
            attachment.ModifiedAtUtc!.Value,
            changes);
        await messageProducer.SendHistoryAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private async Task<AttachmentDeletedIntegrationEvent> PublishDeletedEventsAsync(Attachment attachment, CancellationToken cancellationToken)
    {
        var modifiedEvent = await modifiedEventService.GetModifiedEventAsync(cancellationToken);
        var integrationEvent = new AttachmentDeletedIntegrationEvent(plantProvider.Plant, attachment.Guid,
            attachment.ParentGuid, attachment.GetFullBlobPath(), modifiedEvent.User, modifiedEvent.ModifiedAtUtc);
        
        await messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryDeletedIntegrationEvent(
            $"Attachment {attachment.FileName} deleted",
            attachment.Guid,
            attachment.ParentGuid,
            modifiedEvent.User,
            modifiedEvent.ModifiedAtUtc);
        await messageProducer.SendHistoryAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private static List<IChangedProperty> SetRevisionNumber(Attachment attachment)
    {
        var changes = new List<IChangedProperty>();
        var oldRevision = attachment.RevisionNumber;
        attachment.IncreaseRevisionNumber();
        changes.Add(new ChangedProperty<int>(nameof(Attachment.RevisionNumber), oldRevision, attachment.RevisionNumber, ValueDisplayType.IntAsText));
        return changes;
    }

    private static async Task<string> DetermineContentTypeAsync(Stream stream, string filename, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return await FileMagicMatcher.GetMimeForFileAsync(stream, extension, cancellationToken);
    }
}
