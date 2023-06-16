using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class AttachmentRepository : EntityWithGuidRepository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(CompletionContext context)
        : base(context, context.Attachments, context.Attachments)
    {
    }

    public Task<Attachment?> TryGetAttachmentWithFilenameForSourceAsync(Guid sourceGuid, string fileName)
        => DefaultQuery.SingleOrDefaultAsync(a => a.SourceGuid == sourceGuid && a.FileName == fileName);
}
