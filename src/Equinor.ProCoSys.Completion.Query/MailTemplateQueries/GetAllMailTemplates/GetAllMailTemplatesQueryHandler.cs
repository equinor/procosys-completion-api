using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;

public class GetAllMailTemplatesQueryHandler : IRequestHandler<GetAllMailTemplatesQuery, IEnumerable<MailTemplateDto>>
{
    private readonly IReadOnlyContext _context;

    public GetAllMailTemplatesQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<IEnumerable<MailTemplateDto>> Handle(GetAllMailTemplatesQuery request, CancellationToken cancellationToken)
    {
        var orderedMailTemplates =
            await (from l in _context.QuerySet<MailTemplate>()
                    orderby l.Plant, l.Code
                   select l)
                .TagWith($"{nameof(GetAllMailTemplatesQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        var orderedMailTemplateDtos = orderedMailTemplates
            .Select(mt => new MailTemplateDto(mt.Code, mt.Subject, mt.Body, mt.IsVoided, mt.Plant, mt.IsGlobal()));

        return orderedMailTemplateDtos;
    }
}
