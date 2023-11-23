using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.Links;

public class LinkService : ILinkService
{
    private readonly IReadOnlyContext _context;

    public LinkService(IReadOnlyContext context) => _context = context;

    public async Task<IEnumerable<LinkDto>> GetAllForParentAsync(
        Guid parentGuid,
        CancellationToken cancellationToken)
    {
        var links =
            await (from l in _context.QuerySet<Link>()
                   where l.ParentGuid == parentGuid
                   select new LinkDto(l.ParentGuid, l.Guid, l.Title, l.Url, l.RowVersion.ConvertToString()
               ))
                .TagWith($"{nameof(LinkService)}.{nameof(GetAllForParentAsync)}")
                .ToListAsync(cancellationToken);

        return links;
    }
}
