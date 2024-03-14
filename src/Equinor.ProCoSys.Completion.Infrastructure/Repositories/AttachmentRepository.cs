using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class AttachmentRepository(CompletionContext context)
    : EntityWithGuidRepository<Attachment>(context, context.Attachments), IAttachmentRepository
{
    public Task<Attachment?> GetAttachmentWithFileNameForParentAsync(Guid parentGuid, string fileName, CancellationToken cancellationToken)
        => DefaultQuery.SingleOrDefaultAsync(
            a => a.ParentGuid == parentGuid && a.FileName == fileName,
            cancellationToken);

    public async Task<Attachment> GetAttachmentWithLabelsAsync(Guid attachmentGuid, CancellationToken cancellationToken) 
        => await DefaultQuery
               .Include(a => a.Labels)
               .SingleOrDefaultAsync(a => a.Guid == attachmentGuid, cancellationToken)
           ?? throw new EntityNotFoundException<Attachment>(attachmentGuid.ToString());
}
