using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public interface ICommentService
{
    Task<CommentDto> AddAsync(
        string parentType,
        Guid parentGuid,
        string text,
        IEnumerable<Label> labels,
        CancellationToken cancellationToken);
}
