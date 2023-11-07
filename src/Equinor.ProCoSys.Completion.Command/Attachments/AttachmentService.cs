using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.AttachmentDomainEvents;
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
        // 2) Trigger the update of modifiedBy / modifiedAt to be able to log who performed the deletion
        attachment.SetRowVersion(rowVersion);
        _attachmentRepository.Remove(attachment);
        attachment.AddDomainEvent(new AttachmentDeletedDomainEvent(attachment));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Attachment '{AttachmentFileName}' with guid {AttachmentGuid} deleted for {AttachmentParentGuid}",
            attachment.FileName, 
            attachment.Guid, 
            attachment.ParentGuid);
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

    public async Task<bool> ExistsAsync(Guid guid,
        CancellationToken cancellationToken)
        => await _attachmentRepository.ExistsAsync(guid, cancellationToken);
}
