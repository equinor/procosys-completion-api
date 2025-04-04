﻿using System;
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
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.CommentEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommandHandler(
    ILabelRepository labelRepository,
    ICommentRepository commentRepository,
    IPersonRepository personRepository,
    ISyncToPCS4Service syncToPCS4Service,
    ICompletionMailService completionMailService,
    IDeepLinkUtility deepLinkUtility,
    IMessageProducer messageProducer,
    IUnitOfWork unitOfWork,
    ICheckListApiService checkListApiService,
    ILogger<RejectPunchItemCommandHandler> logger,
    IOptionsMonitor<ApplicationOptions> options)
    : PunchUpdateCommandBase, IRequestHandler<RejectPunchItemCommand, string>
{
    public const string RejectReasonPropertyName = "Reject reason";


    public async Task<string> Handle(RejectPunchItemCommand request, CancellationToken cancellationToken)
    {
        var rejectLabel = await labelRepository.GetByTextAsync(options.CurrentValue.RejectLabel, cancellationToken);
        var punchItem = request.PunchItem;

        var change = await RejectAsync(punchItem, request.Comment, cancellationToken);

        var mentions = await personRepository.GetOrCreateManyAsync(request.Mentions, cancellationToken);

        var comment = AddToCommentRepository(punchItem, request.Comment, [rejectLabel], mentions);

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            "Punch item rejected",
            [change],
            cancellationToken);

        var commentCreatedIntegrationEvent =
            await PublishCommentCreatedIntegrationEventsAsync(cancellationToken, comment, punchItem.Plant);

        await SendEmailEventAsync(punchItem, request.Comment, mentions, cancellationToken);

        punchItem.SetRowVersion(request.RowVersion);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} rejected", punchItem.ItemNo,
            punchItem.Guid);

        try
        {
            await syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Sync Reject on PunchItemList with guid {PunchItemGuid}",
                request.PunchItemGuid);
            return punchItem.RowVersion.ConvertToString();
        }

        try
        {
            await syncToPCS4Service.SyncNewCommentAsync(commentCreatedIntegrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error occurred while trying to Sync Reject comment on PunchItemList with guid {PunchItemGuid}",
                request.PunchItemGuid);
            return punchItem.RowVersion.ConvertToString();
        }

        try
        {
            await checkListApiService.RecalculateCheckListStatusAsync(punchItem.Plant, punchItem.CheckListGuid,
                cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error occurred while trying to Recalculate the CheckListStatus for CheckList with Guid {guid}",
                punchItem.CheckListGuid);
        }

        return punchItem.RowVersion.ConvertToString();
    }

    private async Task<CommentCreatedIntegrationEvent> PublishCommentCreatedIntegrationEventsAsync(
        CancellationToken cancellationToken, 
        Comment comment,
        string plant)
    {
        var commentCreatedIntegrationEvent = new CommentCreatedIntegrationEvent(comment, plant);

        await messageProducer.PublishAsync(commentCreatedIntegrationEvent, cancellationToken);
        return commentCreatedIntegrationEvent;
    }

    private Comment AddToCommentRepository(
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

    private async Task SendEmailEventAsync(PunchItem punchItem, string comment, List<Person> mentions, CancellationToken cancellationToken)
    {
        var emailContext = GetEmailContext(punchItem, comment);
        var emailAddresses = mentions.Select(m => m.Email).ToList();

        await completionMailService.SendEmailEventAsync(MailTemplateCode.PunchRejected, emailContext, emailAddresses, cancellationToken);
    }

    private dynamic GetEmailContext(PunchItem punchItem, string comment)
    {
        var emailContext = punchItem.GetEmailContext();
        
        emailContext.Comment = comment;
        emailContext.Url = deepLinkUtility.CreateUrl(punchItem.GetContextName(), punchItem.Guid);

        return emailContext;
    }

    private async Task<IChangedProperty> RejectAsync(
        PunchItem punchItem,
        string comment,
        CancellationToken cancellationToken)
    {
        var currentPerson = await personRepository.GetCurrentPersonAsync(cancellationToken);
        var change = new ChangedProperty<string?>(RejectReasonPropertyName, null, comment);
        punchItem.Reject(currentPerson);

        return change;
    }
}
