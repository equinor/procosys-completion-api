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

    public async Task<IEnumerable<LinkDto>> GetAllForSourceAsync(
        Guid sourceGuid,
        CancellationToken cancellationToken)
    {
        var links =
            await (from l in _context.QuerySet<Link>()
                   where l.SourceGuid == sourceGuid
                   select new LinkDto(l.SourceGuid, l.Guid, l.Title, l.Url, l.RowVersion.ConvertToString()
               ))
                .TagWith($"{nameof(LinkService)}.{nameof(GetAllForSourceAsync)}")
                .ToListAsync(cancellationToken);

        return links;
    }
}
