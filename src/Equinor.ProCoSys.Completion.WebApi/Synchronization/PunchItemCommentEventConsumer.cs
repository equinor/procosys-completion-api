using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PunchItemCommentEventConsumer(
    ILogger<PunchItemCommentEventConsumer> logger,
    IPersonRepository personRepository,
    ICommentRepository commentRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<PunchItemCommentEvent>
{
    public async Task Consume(ConsumeContext<PunchItemCommentEvent> context)
    {
        var busEvent = context.Message;
        ValidateMessage(busEvent);

        if (!await commentRepository.ExistsAsync(busEvent.CommentGuid, context.CancellationToken))
        {
            var comment = await CreateCommentEntityAsync(busEvent, context.CancellationToken);
            commentRepository.Add(comment);
            await unitOfWork.SaveChangesFromSyncAsync(context.CancellationToken);
            logger.LogInformation("{EventName} Message Consumed: {MessageId} \n Guid {Guid}",
                nameof(PunchItemChangeHistoryEvent), context.MessageId, busEvent.CommentGuid);
        }
        else
        {
            logger.LogInformation("{EventName} Message Message Ignored because it already exists: {MessageId} \n Guid {Guid}",
                nameof(PunchItemChangeHistoryEvent), context.MessageId, busEvent.CommentGuid);
        }
    }

    private void ValidateMessage(PunchItemCommentEvent busEvent)
    {
        if (busEvent.CommentGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(PunchItemCommentEvent)} is missing {nameof(PunchItemCommentEvent.CommentGuid)}");
        }
        if (busEvent.PunchItemGuid == Guid.Empty)
        {
            throw new Exception($"{nameof(PunchItemCommentEvent)} is missing {nameof(PunchItemCommentEvent.PunchItemGuid)}");
        }
        if (string.IsNullOrEmpty(busEvent.Comment))
        {
            throw new Exception($"{nameof(PunchItemCommentEvent)} is missing {nameof(PunchItemCommentEvent.Comment)}");
        }
    }

    private async Task<Comment> CreateCommentEntityAsync(PunchItemCommentEvent busEvent, CancellationToken cancellationToken)
    {
        var comment = new Comment(nameof(PunchItem), busEvent.PunchItemGuid, busEvent.Comment, busEvent.CommentGuid);

        var person = await personRepository.GetAsync(busEvent.CreatedByGuid, cancellationToken);
        comment.SetSyncProperties(person, busEvent.CreatedAt);

        return comment;
    }
}

public record PunchItemCommentEvent
(
    Guid PunchItemGuid,
    Guid CommentGuid,
    string Comment,
    Guid CreatedByGuid,
    DateTime CreatedAt
);
