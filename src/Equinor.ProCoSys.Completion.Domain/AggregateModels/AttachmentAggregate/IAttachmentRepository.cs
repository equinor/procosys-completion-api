using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

public interface IAttachmentRepository : IRepositoryWithGuid<Attachment>
{
    Task<Attachment?> GetAttachmentWithFileNameForSourceAsync(Guid sourceGuid, string fileName, CancellationToken cancellationToken);
}
