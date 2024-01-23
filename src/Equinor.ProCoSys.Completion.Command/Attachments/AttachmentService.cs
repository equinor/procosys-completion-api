using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
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
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IPlantProvider plantProvider,
        IUnitOfWork unitOfWork,
        IAzureBlobService azureBlobService,
        IOptionsSnapshot<BlobStorageOptions> blobStorageOptions,
        ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _plantProvider = plantProvider;
        _unitOfWork = unitOfWork;
        _azureBlobService = azureBlobService;
        _blobStorageOptions = blobStorageOptions;
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
        attachment.AddDomainEvent(new NewAttachmentUploadedDomainEvent(attachment));

        await UploadAsync(attachment, content, false, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        attachment.IncreaseRevisionNumber();

        attachment.SetRowVersion(rowVersion);
        attachment.AddDomainEvent(new ExistingAttachmentUploadedAndOverwrittenDomainEvent(attachment));
        
        await UploadAsync(attachment, content, true, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
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

        // Setting RowVersion before delete has 2 missions:
        // 1) Set correct Concurrency
        // 2) Ensure that _unitOfWork.SetAuditDataAsync can set ModifiedBy / ModifiedAt needed in published events
        attachment.SetRowVersion(rowVersion);
        _attachmentRepository.Remove(attachment);
        attachment.AddDomainEvent(new AttachmentDeletedDomainEvent(attachment));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        if (changes.Any())
        {
            attachment.AddDomainEvent(new AttachmentUpdatedDomainEvent(attachment, changes));
        }
        attachment.SetRowVersion(rowVersion);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
}
