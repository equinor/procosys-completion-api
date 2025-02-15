﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;

public class GetAllLabelsQueryHandler : IRequestHandler<GetAllLabelsQuery, IEnumerable<LabelDto>>
{
    private readonly IReadOnlyContext _context;

    public GetAllLabelsQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<IEnumerable<LabelDto>> Handle(GetAllLabelsQuery request, CancellationToken cancellationToken)
    {
        var orderedLabels =
            await (from l in _context.QuerySet<Label>()
                        .Include(l => l.AvailableFor)
                    orderby l.Text
                   select l)
                .TagWith($"{nameof(GetAllLabelsQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        var orderedLabelDtos = orderedLabels
            .Select(l => new LabelDto(l.Text, l.IsVoided, l.AvailableFor.Select(h => h.EntityType.ToString()).Order().ToList()));

        return orderedLabelDtos;
    }
}
