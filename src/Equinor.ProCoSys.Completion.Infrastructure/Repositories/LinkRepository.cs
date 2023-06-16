using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LinkRepository : EntityWithGuidRepository<Link>, ILinkRepository
{
    public LinkRepository(CompletionContext context)
        : base(context, context.Links, context.Links)
    {
    }
}
