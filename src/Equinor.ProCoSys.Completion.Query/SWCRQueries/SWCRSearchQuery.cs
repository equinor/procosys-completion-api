using System.Collections.Generic;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.SWCRQueries;

public class SWCRSearchQuery : IRequest<IEnumerable<SWCRDto>>
{
    public SWCRSearchQuery(string searchPhrase) => SearchPhrase = searchPhrase;

    public string SearchPhrase { get; }
}
