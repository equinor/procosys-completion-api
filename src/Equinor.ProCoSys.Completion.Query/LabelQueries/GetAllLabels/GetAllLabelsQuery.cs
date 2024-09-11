using System.Collections.Generic;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;

public class GetAllLabelsQuery : IRequest<IEnumerable<LabelDto>>
{
}
