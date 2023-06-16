using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class CommentRepository : EntityWithGuidRepository<Comment>, ICommentRepository
{
    public CommentRepository(CompletionContext context)
        : base(context, context.Comments, context.Comments)
    {
    }
}
