using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.CommentEvents;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public class CommentService(
    ICommentRepository commentRepository,
    ICompletionMailService completionMailService,
    IDeepLinkUtility deepLinkUtility,
    ISyncToPCS4Service syncToPCS4Service,
    IMessageProducer messageProducer,
    ILogger<CommentService> logger)
    : ICommentService
{
    public async Task<CommentDto> AddAsync(IUnitOfWork unitOfWork,
        IHaveGuid parentEntity,
        string plant,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions,
        string emailTemplateCode,
        CancellationToken cancellationToken)
    {
        var comment = AddToRepository(parentEntity, text, labels, mentions);

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var commentCreatedEvent = new CommentCreatedIntegrationEvent(comment, plant);

        await messageProducer.PublishAsync(commentCreatedEvent, cancellationToken);

        await SendEmailEventAsync(emailTemplateCode, parentEntity, comment, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Comment with guid {CommentGuid} created for {Type} : {CommentParentGuid}",
            comment.Guid,
            comment.ParentType,
            comment.ParentGuid);

        try
        {
            await syncToPCS4Service.SyncNewCommentAsync(commentCreatedEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Sync Create on Comment with guid {guid}", commentCreatedEvent.Guid);
        }

        return new CommentDto(comment.Guid, comment.RowVersion.ConvertToString());
    }
    
    private Comment AddToRepository(
        IHaveGuid parentEntity,
        string text,
        IEnumerable<Label> labels,
        IEnumerable<Person> mentions)
    {
        var comment = new Comment(parentEntity.GetContextName(), parentEntity.Guid, text);
        comment.UpdateLabels(labels.ToList());
        comment.SetMentions(mentions.ToList());

        commentRepository.Add(comment);

        return comment;
    }

    private async Task SendEmailEventAsync(
        string emailTemplateCode,
        IHaveGuid parentEntity,
        Comment comment, 
        CancellationToken cancellationToken)
    {
        var emailContext = parentEntity.GetEmailContext();
        emailContext.Comment = comment;
        emailContext.Url = deepLinkUtility.CreateUrl(parentEntity.GetContextName(), parentEntity.Guid);
        
        var emailAddresses = comment.Mentions.Select(m => m.Email).ToList();
        await completionMailService.SendEmailEventAsync(emailTemplateCode, emailContext, emailAddresses, cancellationToken);
    }
}
