using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

public interface IAttachmentRepository : IRepositoryWithGuid<Attachment>
{
    Task<Attachment?> GetAttachmentWithFileNameForParentAsync(Guid parentGuid, string fileName, CancellationToken cancellationToken);
    Task<Attachment> GetAttachmentWithLabelsAsync(Guid attachmentGuid, CancellationToken cancellationToken);
}
