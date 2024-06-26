using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

public interface ICommentRepository : IRepositoryWithGuid<Comment>
{
    Task<IEnumerable<Comment>> GetAllByParentGuidAsync(Guid parentGuid, CancellationToken cancellationToken);
}
