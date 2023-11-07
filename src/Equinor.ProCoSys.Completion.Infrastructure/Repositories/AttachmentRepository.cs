using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class AttachmentRepository : EntityWithGuidRepository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(CompletionContext context)
        : base(context, context.Attachments)
    {
    }

    public Task<Attachment?> GetAttachmentWithFileNameForParentAsync(Guid parentGuid, string fileName)
        => DefaultQuery.SingleOrDefaultAsync(a => a.ParentGuid == parentGuid && a.FileName == fileName);
}
