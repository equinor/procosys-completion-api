using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

namespace Equinor.ProCoSys.Completion.Command.Attachments;

public interface IAttachmentService
{
    Task<AttachmentDto> UploadNewAsync(
        string project,
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    Task<string> UploadOverwriteAsync(
        string parentType,
        Guid parentGuid,
        string fileName,
        Stream content,
        string contentType,
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
    
    Task<string> UpdateAsync(
        Guid guid,
        string description,
        IEnumerable<Label> labels,
        string rowVersion,
        CancellationToken cancellationToken);

    Task<List<AttachmentDto>> CopyAttachments(
        List<Attachment> attachments,
        string parentType,
        Guid parentGuid,
        string project,
        CancellationToken cancellationToken);
}
