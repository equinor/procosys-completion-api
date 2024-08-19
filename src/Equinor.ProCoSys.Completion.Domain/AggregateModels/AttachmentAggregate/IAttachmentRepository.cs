using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

public interface IAttachmentRepository : IRepositoryWithGuid<Attachment>
{
    Task<Attachment?> GetAttachmentWithFileNameForParentAsync(Guid parentGuid, string fileName, CancellationToken cancellationToken);
    Task<Attachment> GetAttachmentWithLabelsAsync(Guid attachmentGuid, CancellationToken cancellationToken);
    Task<IEnumerable<Attachment>> GetAllByParentGuidAsync(Guid parentGuid, CancellationToken cancellationToken);
    Task<List<Attachment>> GetAttachmentsByParentGuid(Guid parentGuid, CancellationToken cancellationToken);
}
