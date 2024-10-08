﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.Attachments;

public interface IAttachmentService
{
    Task<IEnumerable<AttachmentDto>> GetAllForParentAsync(
        Guid parent,
        CancellationToken cancellationToken,
        string? fromIpAddress = null,
        string? toIpAddress = null
        );

    Task<Uri?> GetDownloadUriAsync(
        Guid guid,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        Guid guid,
        CancellationToken cancellationToken);
}
