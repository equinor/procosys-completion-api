using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public interface ICommentService
{
    Task<CommentDto> AddAsync(
        string parentType,
        Guid parentGuid,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions,
        CancellationToken cancellationToken);
    
    Task<CommentDto> AddAsync(
        string parentType,
        Guid parentGuid,
        string text,
        Label label,
        IEnumerable<Person> mentions,
        CancellationToken cancellationToken);
}
