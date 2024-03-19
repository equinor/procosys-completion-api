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
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<AttachmentService> _logger;


    private static readonly Dictionary<string, byte[]> s_fileSignature = new()
    {
        { ".pdf", [0x25, 0x50, 0x44, 0x46] },
        { ".jpg", [0xFF, 0xD8, 0xFF] },
        { ".png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A] },
        { ".gif", [0x47, 0x49, 0x46, 0x38] },
        { ".bmp", [0x42, 0x4D] },
        { ".tiff", [0x49, 0x49, 0x2A, 0x00] }, // TIFF image (little-endian)
        { ".tif", [0x4D, 0x4D, 0x00, 0x2A] }, // TIFF image (big-endian)
        { ".webp", [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50] },
        { ".docx", [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00] }, // DOCX (also valid for other Office Open XML formats like .xlsx and .pptx)
        { ".xlsx", [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00] } // XLSX (also valid for other Office Open XML formats)
    };

    private static readonly Dictionary <string, string> s_contentTypeMappings = new()
    {
        { ".pdf", "application/pdf" },
        { ".jpg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".tiff", "image/tiff" },
        { ".tif", "image/tiff" },
        { ".webp", "image/webp" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
    };

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IPlantProvider plantProvider,
        IUnitOfWork unitOfWork,
        IAzureBlobService azureBlobService,
        IOptionsSnapshot<BlobStorageOptions> blobStorageOptions,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _plantProvider = plantProvider;
        _unitOfWork = unitOfWork;
        _azureBlobService = azureBlobService;
        _blobStorageOptions = blobStorageOptions;
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task<AttachmentDto> UploadNewAsync(
        string project,
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetAttachmentWithFileNameForParentAsync(parentGuid, fileName, cancellationToken);

        if (attachment is not null)
        {
            throw new Exception($"{parentType} {parentGuid} already has an attachment with filename {fileName}");
        }

        attachment = new Attachment(
            project,
            parentType,
            parentGuid,
            fileName);
        _attachmentRepository.Add(attachment);

        var verifiedContentType = await DetermineContentTypeAsync(content, fileName, contentType);
        await UploadAsync(attachment, content, false, verifiedContentType, cancellationToken);

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
        string contentType,
        string rowVersion,
        CancellationToken cancellationToken)
    {
        var attachment = await _attachmentRepository.GetAttachmentWithFileNameForParentAsync(parentGuid, fileName, cancellationToken);

        if (attachment is null)
        {
            throw new Exception($"{parentType} {parentGuid} don't have an attachment with filename {fileName}");
        }

        var changes = SetRevisionNumber(attachment);

        var verifiedContentType = await DetermineContentTypeAsync(content, fileName, contentType);
        await UploadAsync(attachment, content, true, verifiedContentType, cancellationToken);

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
        var attachment = await _attachmentRepository.GetAttachmentWithLabelsAsync(guid, cancellationToken);
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
        string contentType,
        CancellationToken cancellationToken)
    {
        var fullBlobPath = attachment.GetFullBlobPath();
        await _azureBlobService.UploadAsync(
            _blobStorageOptions.Value.BlobContainer,
            fullBlobPath,
            content,
            contentType,
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
        await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

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
        await _integrationEventPublisher.PublishAsync(historyEvent, cancellationToken);
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
        await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryUpdatedIntegrationEvent(
            displayName,
            attachment.Guid,
            attachment.ParentGuid,
            new User(attachment.ModifiedBy!.Guid, attachment.ModifiedBy!.GetFullName()),
            attachment.ModifiedAtUtc!.Value,
            changes);
        await _integrationEventPublisher.PublishAsync(historyEvent, cancellationToken);
        return integrationEvent;
    }

    private async Task<AttachmentDeletedIntegrationEvent> PublishDeletedEventsAsync(Attachment attachment, CancellationToken cancellationToken)
    {
        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = new AttachmentDeletedIntegrationEvent(attachment, _plantProvider.Plant);
        await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryDeletedIntegrationEvent(
            $"Attachment {attachment.FileName} deleted",
            attachment.Guid,
            attachment.ParentGuid,
            // Our entities don't have DeletedByOid / DeletedAtUtc ...
            // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
            new User(attachment.ModifiedBy!.Guid, attachment.ModifiedBy!.GetFullName()),
            attachment.ModifiedAtUtc!.Value);
        await _integrationEventPublisher.PublishAsync(historyEvent, cancellationToken);
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

    public static async Task<string> DetermineContentTypeAsync(Stream stream, string filename, string providedContentType)
    {
        // Verify content type using file extension
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        if (!s_fileSignature.TryGetValue(extension, out var expectedSignature))
        {
            // Extension not supported, return default content type
            return "application/octet-stream";
        }

        // Only check the signature if the provided content type matches the extension
        if (IsContentTypeMatchingExtension(providedContentType, extension))
        {
            // Reset position to the start of the stream
            if (stream.CanSeek)
            { 
                stream.Seek(0, SeekOrigin.Begin); 
            }

            var actualSignature = new byte[expectedSignature.Length];
            var bytesRead = await stream.ReadAsync(actualSignature, 0, actualSignature.Length);
            if (bytesRead == expectedSignature.Length && actualSignature.SequenceEqual(expectedSignature))
            {
                // Signature matches, consider verified
                // Reset position again
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                return providedContentType;
            }
        }

        // If the content type does not match the signature, return default
        return "application/octet-stream";
    }

    private static bool IsContentTypeMatchingExtension(string contentType, string extension)
    {
        if (s_contentTypeMappings.TryGetValue(extension, out var expectedContentType))
        {
            return contentType.Equals(expectedContentType, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
