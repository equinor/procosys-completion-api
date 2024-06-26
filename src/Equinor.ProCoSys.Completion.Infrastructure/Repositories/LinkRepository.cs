using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LinkRepository(CompletionContext context)
    : EntityWithGuidRepository<Link>(context, context.Links), ILinkRepository
{
    public async Task<IEnumerable<Link>> GetAllByParentGuidAsync(Guid parentGuid, CancellationToken cancellationToken) 
        => await DefaultQueryable.Where(l => l.ParentGuid == parentGuid).ToListAsync(cancellationToken);
}
