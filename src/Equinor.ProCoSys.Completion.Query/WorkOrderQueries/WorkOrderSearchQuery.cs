using System.Collections.Generic;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.WorkOrderQueries;

public class WorkOrderSearchQuery : IRequest<IEnumerable<WorkOrderDto>>
{
    public WorkOrderSearchQuery(string searchPhrase) => SearchPhrase = searchPhrase;

    public string SearchPhrase { get; }
}
