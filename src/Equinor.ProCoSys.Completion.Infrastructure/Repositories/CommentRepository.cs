using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class CommentRepository(CompletionContext context)
    : EntityWithGuidRepository<Comment>(context, context.Comments), ICommentRepository
{
    public async Task<IEnumerable<Comment>> GetAllByParentGuidAsync(Guid parentGuid, CancellationToken cancellationToken)
        => await DefaultQueryable.Where(c => c.ParentGuid == parentGuid).ToListAsync(cancellationToken);
    
    
}
