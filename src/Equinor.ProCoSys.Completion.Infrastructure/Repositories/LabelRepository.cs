using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LabelRepository : EntityRepository<Label>, ILabelRepository
{
    public LabelRepository(CompletionContext context)
        : base(context, context.Labels)
    {
    }
}
