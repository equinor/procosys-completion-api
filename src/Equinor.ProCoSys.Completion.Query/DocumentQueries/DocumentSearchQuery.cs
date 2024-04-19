using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.DocumentQueries;

public class DocumentSearchQuery : IRequest<Result<IEnumerable<DocumentDto>>>
{
    public DocumentSearchQuery(string searchPhrase) => SearchPhrase = searchPhrase;

    public string SearchPhrase { get; }
}
