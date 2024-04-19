using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.DocumentQueries;

public class DocumentSearchQueryHandler : IRequestHandler<DocumentSearchQuery, Result<IEnumerable<DocumentDto>>>
{
    private readonly IReadOnlyContext _context;

    public DocumentSearchQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<DocumentDto>>> Handle(DocumentSearchQuery request, CancellationToken cancellationToken)
    {
        var documents = await (from document in _context.QuerySet<Document>()
                where document.No.ToString().Contains(request.SearchPhrase)
                select new DocumentDto(
                    document.Guid,
                    document.No
                )).TagWith($"{nameof(DocumentSearchQueryHandler)}.{nameof(Handle)}")
            .ToListAsync(cancellationToken);

        var orderedDocuments = documents.OrderBy(l => l.No);

        return new SuccessResult<IEnumerable<DocumentDto>>(orderedDocuments);
    }
}
