using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.Comment;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.CommentEvents;

public record CommentCreatedIntegrationEvent(
    Guid Guid,
    string Plant,
    Guid ParentGuid,
    User CreatedBy,
    DateTime CreatedAtUtc,
    string Text,
    IEnumerable<string> Labels
    ) : ICommentCreatedEventV1
{
    public CommentCreatedIntegrationEvent(Comment comment, string plant) : this(
        comment.Guid,
        plant,
        comment.ParentGuid,
        new User(comment.CreatedBy.Guid, comment.CreatedBy.GetFullName()),
        comment.CreatedAtUtc,
        comment.Text,
        comment.Labels.Select(x => x.Text))
    { }
}


