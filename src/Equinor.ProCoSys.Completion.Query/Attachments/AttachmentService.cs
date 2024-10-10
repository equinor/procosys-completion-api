using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Query.UserDelegationProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Query.Attachments;

public class AttachmentService(
    IReadOnlyContext context,
    IAzureBlobService azureBlobService,
    IUserDelegationProvider userDelegationProvider,
    IOptionsSnapshot<BlobStorageOptions> blobStorageOptions,
    IOptionsSnapshot<ApplicationOptions> applicationOptions) : IAttachmentService
{
    private readonly IReadOnlyContext _context = context;
    private readonly IAzureBlobService _azureBlobService = azureBlobService;
    private readonly IUserDelegationProvider _userDelegationProvider = userDelegationProvider;
    private readonly IOptionsSnapshot<BlobStorageOptions> _blobStorageOptions = blobStorageOptions;
    private readonly IOptionsSnapshot<ApplicationOptions> _applicationOptions = applicationOptions;

    public async Task<IEnumerable<AttachmentDto>> GetAllForParentAsync(
        Guid parent,
        CancellationToken cancellationToken,
        string? fromIpAddress = null,
        string? toIpAddress = null)
    {
        var attachments =
            await (from a in _context.QuerySet<Attachment>()
                    .Include(a => a.Labels.Where(l => !l.IsVoided))
                    .Include(a => a.CreatedBy)
                    .Include(a => a.ModifiedBy)
                   where a.ParentGuid == parent
                   select a)
                .TagWith($"{nameof(AttachmentService)}.{nameof(GetAllForParentAsync)}")
                .ToListAsync(cancellationToken);

        var sasUris = GenerateDownloadSasUris(attachments, fromIpAddress, toIpAddress);

        var attachmentsDtos = 
                attachments.Select(a => new AttachmentDto(
                    a.ParentGuid,
                    a.Guid,
                    a.GetFullBlobPath(),
                    sasUris[a.Guid].ToString(),
                    a.FileName,
                    a.Description,
                    a.GetOrderedNonVoidedLabels().Select(l => l.Text).ToList(),
                    new PersonDto(
                        a.CreatedBy.Guid,
                        a.CreatedBy.FirstName,
                        a.CreatedBy.LastName,
                        a.CreatedBy.UserName,
                        a.CreatedBy.Email),
                    a.CreatedAtUtc,
                    a.ModifiedBy != null ?
                        new PersonDto(
                            a.ModifiedBy.Guid,
                            a.ModifiedBy.FirstName,
                            a.ModifiedBy.LastName,
                            a.ModifiedBy.UserName,
                            a.ModifiedBy.Email) : null,
                    a.ModifiedAtUtc,
                    a.RowVersion.ConvertToString()
                ));
        return attachmentsDtos;
    }

    public async Task<Uri?> GetDownloadUriAsync(Guid guid, CancellationToken cancellationToken)
    {
        var attachment = await GetAttachmentAsync(guid, cancellationToken);

        if (attachment is null)
        {
            return null;
        }

        var uri = GetSasUri(attachment);
        return uri;
    }

    public async Task<bool> ExistsAsync(Guid guid, CancellationToken cancellationToken)
    {
        var attachment = await GetAttachmentAsync(guid, cancellationToken);
        return attachment is not null;
    }

    private async Task<Attachment?> GetAttachmentAsync(Guid guid, CancellationToken cancellationToken)
    {
        var attachment = await
            (from a in _context.QuerySet<Attachment>()
                where a.Guid == guid
                select a).SingleOrDefaultAsync(cancellationToken);
        return attachment;
    }

    private Dictionary<Guid, Uri> GenerateDownloadSasUris(
        List<Attachment> attachments, 
        string? fromIpAddress = null, 
        string? toIpAddress = null) =>
        attachments.Select(attachment => new
        {
            uri = GetSasUri(attachment, fromIpAddress, toIpAddress),
            guid = attachment.Guid    
        }).ToDictionary(item => item.guid, item => item.uri);


    private Uri GetSasUri(Attachment x, string? fromIpAddress = null, string? toIpAddress = null) {
        var now = TimeService.UtcNow;
        return _azureBlobService.GetDownloadSasUri(
            _blobStorageOptions.Value.BlobContainer,
            x.GetFullBlobPath(),
            new DateTimeOffset(now.AddMinutes(_blobStorageOptions.Value.BlobClockSkewMinutes * -1)),
            new DateTimeOffset(now.AddMinutes(_blobStorageOptions.Value.BlobClockSkewMinutes)),
            _userDelegationProvider.GetUserDelegationKey(),
            _applicationOptions.Value.DevOnLocalhost ? null : fromIpAddress,
            _applicationOptions.Value.DevOnLocalhost ? null : toIpAddress
        );
    }
}
