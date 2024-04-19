using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.WorkOrderQueries;

public class WorkOrderSearchQuery : IRequest<Result<IEnumerable<WorkOrderDto>>>
{
    public WorkOrderSearchQuery(string searchPhrase) => SearchPhrase = searchPhrase;

    public string SearchPhrase { get; }
}
