using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Query.Comments;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetAllForParentAsync(
        Guid parentGuid,
        CancellationToken cancellationToken);
}
