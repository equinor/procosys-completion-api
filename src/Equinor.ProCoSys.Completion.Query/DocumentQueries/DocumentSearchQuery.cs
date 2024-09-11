using System.Collections.Generic;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.DocumentQueries;

public class DocumentSearchQuery : IRequest<IEnumerable<DocumentDto>>
{
    public DocumentSearchQuery(string searchPhrase) => SearchPhrase = searchPhrase;

    public string SearchPhrase { get; }
}
