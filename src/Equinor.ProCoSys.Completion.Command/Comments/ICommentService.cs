using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public interface ICommentService
{
    Task<CommentDto> AddAsync(
        string sourceType,
        Guid sourceGuid,
        string text,
        CancellationToken cancellationToken);
}
