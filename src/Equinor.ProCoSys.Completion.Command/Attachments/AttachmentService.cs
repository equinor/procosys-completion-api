using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
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

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IPlantProvider _plantProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IOptionsSnapshot<BlobStorageOptions> _blobStorageOptions;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IPlantProvider plantProvider,
        IUnitOfWork unitOfWork,
        IAzureBlobService azureBlobService,
        IOptionsSnapshot<BlobStorageOptions> blobStorageOptions,
        IIntegrationEventPublisher eventPublisher,
        ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _plantProvider = plantProvider;
        _unitOfWork = unitOfWork;
        _azureBlobService = azureBlobService;
        _blobStorageOptions = blobStorageOptions;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<AttachmentDto> UploadNewAsync(
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetAttachmentWithFileNameForParentAsync(parentGuid, fileName, cancellationToken);

        if (attachment is not null)
        {
            throw new Exception($"{parentType} {parentGuid} already has an attachment with filename {fileName}");
        }

        attachment = new Attachment(
            parentType,
            parentGuid,
            _plantProvider.Plant,
            fileName);
        _attachmentRepository.Add(attachment);

        await UploadAsync(attachment, content, false, cancellationToken);

        // ReSharper disable once UnusedVariable
        var integrationEvent = await PublishCreatedEventsAsync(attachment, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Todo 109496 sync with pcs4 goes here
        // await _syncToPCS4Service.SyncNewObjectAsync(SyncToPCS4Service.Attachment, integrationEvent, attachment.Plant, cancellationToken);

        return new AttachmentDto(attachment.Guid, attachment.RowVersion.ConvertToString());
    }

    public async Task<string> UploadOverwriteAsync(
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetAttachmentWithFileNameForParentAsync(parentGuid, fileName, cancellationToken);

        if (attachment is null)
        {
            throw new Exception($"{parentType} {parentGuid} don't have an attachment with filename {fileName}");
        }

        var changes = SetRevisionNumber(attachment);

        await UploadAsync(attachment, content, true, cancellationToken);

        // ReSharper disable once UnusedVariable
        var integrationEvent = await PublishUpdatedEventsAsync(
            $"Attachment {attachment.FileName} uploaded again", 
            attachment, 
            changes, 
            cancellationToken);

        attachment.SetRowVersion(rowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Todo 109496 sync with pcs4 goes here
        // await _syncToPCS4Service.SyncObjectUpdateAsync(SyncToPCS4Service.Attachment, integrationEvent, attachment.Plant, cancellationToken);

        return attachment.RowVersion.ConvertToString();
    }

    public async Task<bool> FileNameExistsForParentAsync(Guid parentGuid, string fileName, CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetAttachmentWithFileNameForParentAsync(parentGuid, fileName, cancellationToken);
        return attachment is not null;
    }

    public async Task DeleteAsync(
        Guid guid,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetAsync(guid, cancellationToken);

        var fullBlobPath = attachment.GetFullBlobPath();
        await _azureBlobService.DeleteAsync(
            _blobStorageOptions.Value.BlobContainer,
            fullBlobPath,
            cancellationToken);

        _attachmentRepository.Remove(attachment);

        // ReSharper disable once UnusedVariable
        var integrationEvent = await PublishDeletedEventsAsync(attachment, cancellationToken);

        // Setting RowVersion before delete has 2 missions:
        // 1) Set correct Concurrency
        // 2) Ensure that _unitOfWork.SetAuditDataAsync can set ModifiedBy / ModifiedAt needed in published events
        attachment.SetRowVersion(rowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Todo 109496 sync with pcs4 goes here
        // await _syncToPCS4Service.SyncObjectDeletionAsync(SyncToPCS4Service.Attachment, integrationEvent, attachment.Plant, cancellationToken);

        _logger.LogInformation("Attachment '{AttachmentFileName}' with guid {AttachmentGuid} deleted for {AttachmentParentGuid}",
            attachment.FileName, 
            attachment.Guid, 
            attachment.ParentGuid);
    }

    public async Task<bool> ExistsAsync(Guid guid,
        CancellationToken cancellationToken)
        => await _attachmentRepository.ExistsAsync(guid, cancellationToken);

    public async Task<string> UpdateAsync(
        Guid guid,
        string description,
        IEnumerable<Label> labels,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetAsync(guid, cancellationToken);
        attachment.UpdateLabels(labels.ToList());

        var changes = UpdateAttachment(attachment, description);
        // ReSharper disable once NotAccessedVariable
        AttachmentUpdatedIntegrationEvent integrationEvent;
        if (changes.Any())
        {
            // ReSharper disable once RedundantAssignment
            integrationEvent = await PublishUpdatedEventsAsync($"Attachment {attachment.FileName} updated", attachment, changes, cancellationToken);
        }
        attachment.SetRowVersion(rowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Todo 109496 sync with pcs4 goes here
        // await _syncToPCS4Service.SyncObjectUpdateAsync(SyncToPCS4Service.Attachment, integrationEvent, attachment.Plant, cancellationToken);

        return attachment.RowVersion.ConvertToString();
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
        CancellationToken cancellationToken)
    {
        var fullBlobPath = attachment.GetFullBlobPath();
        await _azureBlobService.UploadAsync(
            _blobStorageOptions.Value.BlobContainer,
            fullBlobPath,
            content,
            overwriteIfExists,
            cancellationToken);

        _logger.LogInformation("Attachment '{AttachmentFileName}' with guid {AttachmentGuid} uploaded for {AttachmentParentGuid}",
            attachment.FileName,
            attachment.Guid,
            attachment.ParentGuid);
    }

    private async Task<AttachmentCreatedIntegrationEvent> PublishCreatedEventsAsync(Attachment attachment, CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = new AttachmentCreatedIntegrationEvent(attachment, _plantProvider.Plant);
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var properties = new List<IProperty>
        {
            new Property(nameof(Attachment.FileName), attachment.FileName)
        };
        var historyEvent = new HistoryCreatedIntegrationEvent(
            _plantProvider.Plant,
            $"Attachment {attachment.FileName} uploaded",
            attachment.Guid,
            attachment.ParentGuid,
            new User(attachment.CreatedBy.Guid, attachment.CreatedBy.GetFullName()),
            attachment.CreatedAtUtc,
            properties);
        await _eventPublisher.PublishAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private async Task<AttachmentUpdatedIntegrationEvent> PublishUpdatedEventsAsync(
        string displayName,
        Attachment attachment, 
        List<IChangedProperty> changes, 
        CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = new AttachmentUpdatedIntegrationEvent(attachment, _plantProvider.Plant);
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryUpdatedIntegrationEvent(
            _plantProvider.Plant,
            displayName,
            attachment.Guid,
            new User(attachment.ModifiedBy!.Guid, attachment.ModifiedBy!.GetFullName()),
            attachment.ModifiedAtUtc!.Value,
            changes);
        await _eventPublisher.PublishAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private async Task<AttachmentDeletedIntegrationEvent> PublishDeletedEventsAsync(Attachment attachment, CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = new AttachmentDeletedIntegrationEvent(attachment, _plantProvider.Plant);
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryDeletedIntegrationEvent(
            _plantProvider.Plant,
            $"Attachment {attachment.FileName} deleted",
            attachment.Guid,
            attachment.ParentGuid,
            // Our entities don't have DeletedByOid / DeletedAtUtc ...
            // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
            new User(attachment.ModifiedBy!.Guid, attachment.ModifiedBy!.GetFullName()),
            attachment.ModifiedAtUtc!.Value);
        await _eventPublisher.PublishAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private static List<IChangedProperty> SetRevisionNumber(Attachment attachment)
    {
        var changes = new List<IChangedProperty>();
        var oldRevision = attachment.RevisionNumber;
        attachment.IncreaseRevisionNumber();
        changes.Add(new ChangedProperty<int>(nameof(Attachment.RevisionNumber), oldRevision, attachment.RevisionNumber));
        return changes;
    }
}
