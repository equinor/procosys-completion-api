using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntityType;

public class GetLabelsForEntityTypeQuery : IRequest<IEnumerable<string>>
{
    public GetLabelsForEntityTypeQuery(EntityTypeWithLabel entityType) => EntityType = entityType;

    public EntityTypeWithLabel EntityType { get; }
}
