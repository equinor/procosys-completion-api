using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntityType;

public class GetLabelsForEntityTypeQuery : IRequest<Result<IEnumerable<string>>>
{
    public GetLabelsForEntityTypeQuery(EntityTypeWithLabels entityType) => EntityType = entityType;

    public EntityTypeWithLabels EntityType { get; }
}
