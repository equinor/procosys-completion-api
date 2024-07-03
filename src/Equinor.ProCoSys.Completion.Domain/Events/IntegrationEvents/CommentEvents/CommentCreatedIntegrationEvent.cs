using System;
using System.Collections.Generic;
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
    ) : ICommentCreatedEventV1;

