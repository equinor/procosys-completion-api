using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.SWCRQueries;

public class SWCRSearchQuery : IRequest<Result<IEnumerable<SWCRDto>>>
{

    public SWCRSearchQuery(string searchPhrase) => SearchPhrase = searchPhrase;

    public string SearchPhrase { get; }
}
