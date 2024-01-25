using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;

public class GetAllLabelsQuery : IRequest<Result<IEnumerable<LabelDto>>>
{
}
