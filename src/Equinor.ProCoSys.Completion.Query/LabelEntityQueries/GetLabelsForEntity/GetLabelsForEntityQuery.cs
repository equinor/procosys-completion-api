using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntity;

public class GetLabelsForEntityQuery : IRequest<Result<IEnumerable<string>>>
{
    public GetLabelsForEntityQuery(EntityWithLabelType entityWithLabelsType) => EntityWithLabelsType = entityWithLabelsType;

    public EntityWithLabelType EntityWithLabelsType { get; }
}
