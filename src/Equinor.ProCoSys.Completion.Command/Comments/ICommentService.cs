using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public interface ICommentService
{
    Task<CommentDto> AddAndSaveAsync(
        IUnitOfWork unitOfWork,
        string parentType,
        Guid parentGuid,
        string text,
        IEnumerable<Label> labels,
        CancellationToken cancellationToken);
    
    Guid Add(string parentType, Guid parentGuid, string text, IEnumerable<Label> labels);
}
