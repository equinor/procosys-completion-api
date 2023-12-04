using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelHostAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LabelQueries.GetLabels;

public class GetLabelsForHostQuery : IRequest<Result<IEnumerable<string>>>
{
    public GetLabelsForHostQuery(HostType hostType) => HostType = hostType;

    public HostType HostType { get; }
}
