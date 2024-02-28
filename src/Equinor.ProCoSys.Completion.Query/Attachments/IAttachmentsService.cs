using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.Attachments;

public interface IAttachmentService
{
    Task<IEnumerable<AttachmentDto>> GetAllForParentAsync(
        Guid parent,
        CancellationToken cancellaionToken,
        string? fromIPAddress = null,
        string? toIPAddress = null
        );

    Task<Uri?> GetDownloadUriAsync(
        Guid guid,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        Guid guid,
        CancellationToken cancellationToken);
}
