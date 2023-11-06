using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Attachments;

public interface IAttachmentService
{
    Task<AttachmentDto> UploadNewAsync(
        string sourceType,
        Guid sourceGuid,
        string fileName,
        Stream content,
        CancellationToken cancellationToken);

    Task<string> UploadOverwriteAsync(
        string sourceType,
        Guid sourceGuid,
        string fileName,
        Stream content,
        string rowVersion,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid guid,
        string rowVersion,
        CancellationToken cancellationToken);

    Task<bool> FileNameExistsForSourceAsync(
        Guid sourceGuid,
        string fileName,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid guid, CancellationToken cancellationToken);
}
