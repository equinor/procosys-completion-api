using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

public interface IAttachmentRepository : IRepositoryWithGuid<Attachment>
{
    Task<Attachment?> GetAttachmentWithFilenameForSourceAsync(Guid sourceGuid, string fileName);
}
