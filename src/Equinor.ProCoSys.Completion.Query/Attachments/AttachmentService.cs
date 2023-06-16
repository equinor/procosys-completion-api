using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Time;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Query.Attachments;

public class AttachmentService : IAttachmentService
{
    private readonly IReadOnlyContext _context;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IOptionsSnapshot<BlobStorageOptions> _blobStorageOptions;

    public AttachmentService(
        IReadOnlyContext context,
        IAzureBlobService azureBlobService,
        IOptionsSnapshot<BlobStorageOptions> blobStorageOptions)
    {
        _context = context;
        _azureBlobService = azureBlobService;
        _blobStorageOptions = blobStorageOptions;
    }

    public async Task<IEnumerable<AttachmentDto>> GetAllForSourceAsync(
        Guid sourceGuid,
        CancellationToken cancellationToken)
    {
        var attachments =
            await (from a in _context.QuerySet<Attachment>()
                    join createdByUser in _context.QuerySet<Person>()
                        on a.CreatedById equals createdByUser.Id
                    from modifiedByUser in _context.QuerySet<Person>()
                        .Where(p => p.Id == a.ModifiedById).DefaultIfEmpty() //left join!
                   where a.SourceGuid == sourceGuid
                   select new AttachmentDto(
                       a.SourceGuid,
                       a.Guid,
                       a.GetFullBlobPath(),
                       a.FileName,
                       new PersonDto(
                           createdByUser.Guid,
                           createdByUser.FirstName,
                           createdByUser.LastName,
                           createdByUser.UserName,
                           createdByUser.Email),
                       a.CreatedAtUtc,
                       modifiedByUser != null ? 
                           new PersonDto(
                               modifiedByUser.Guid,
                               modifiedByUser.FirstName,
                               modifiedByUser.LastName,
                               modifiedByUser.UserName,
                               modifiedByUser.Email) : null, 
                       a.ModifiedAtUtc,
                       a.RowVersion.ConvertToString()
               ))
                .TagWith($"{nameof(AttachmentService)}.{nameof(GetAllForSourceAsync)}")
                .ToListAsync(cancellationToken);

        return attachments;
    }

    public async Task<Uri?> TryGetDownloadUriAsync(Guid guid, CancellationToken cancellationToken)
    {
        var attachment = await
            (from a in _context.QuerySet<Attachment>()
                where a.Guid == guid
                select a).SingleOrDefaultAsync(cancellationToken);

        if (attachment == null)
        {
            return null;
        }

        var now = TimeService.UtcNow;
        var fullBlobPath = attachment.GetFullBlobPath();
        var uri = _azureBlobService.GetDownloadSasUri(
            _blobStorageOptions.Value.BlobContainer,
            fullBlobPath,
            new DateTimeOffset(now.AddMinutes(_blobStorageOptions.Value.BlobClockSkewMinutes * -1)),
            new DateTimeOffset(now.AddMinutes(_blobStorageOptions.Value.BlobClockSkewMinutes)));
        return uri;
    }
}
