using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Attachments;

public interface IAttachmentService
{
    Task<AttachmentDto> UploadNewAsync(
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        CancellationToken cancellationToken);

    Task<string> UploadOverwriteAsync(
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        string rowVersion,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid guid,
        string rowVersion,
        CancellationToken cancellationToken);

    Task<bool> FileNameExistsForParentAsync(
        Guid parentGuid,
        string fileName,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid guid, CancellationToken cancellationToken);
}
