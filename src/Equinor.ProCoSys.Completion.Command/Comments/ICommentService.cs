using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public interface ICommentService
{
    Task<CommentDto> AddAndSaveAsync(
        IUnitOfWork unitOfWork,
        string parentType,
        Guid parentGuid,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions,
        CancellationToken cancellationToken);
    
    Guid Add(
        string parentType,
        Guid parentGuid,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions);
}
