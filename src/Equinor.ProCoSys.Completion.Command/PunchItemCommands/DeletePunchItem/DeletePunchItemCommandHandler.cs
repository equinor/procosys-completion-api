using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using Microsoft.Extensions.Logging;
using User = Equinor.ProCoSys.Completion.MessageContracts.User;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;

public class DeletePunchItemCommandHandler(
    IPunchItemRepository punchItemRepository,
    ISyncToPCS4Service syncToPCS4Service,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ICheckListApiService checkListApiService,
    ILogger<DeletePunchItemCommandHandler> logger,
    ICommentRepository commentsRepository,
    IAttachmentRepository attachmentRepository)
    : IRequestHandler<DeletePunchItemCommand, Unit>
{
    public async Task<Unit> Handle(DeletePunchItemCommand request, CancellationToken cancellationToken)
    {
        var comments = await commentsRepository.GetAllByParentGuidAsync(request.PunchItemGuid, cancellationToken);
        foreach (var comment in comments)
        {
            commentsRepository.Remove(comment);
        }
        var attachments = await attachmentRepository.GetAllByParentGuidAsync(request.PunchItemGuid, cancellationToken);
        foreach (var attachment in attachments)
        {
            await messageProducer.PublishAsync(new AttachmentDeletedByPunchItemIntegrationEvent(attachment.Guid,attachment.GetFullBlobPath()), cancellationToken);
            attachmentRepository.Remove(attachment);
        }
        var punchItem = request.PunchItem;
        
        if (request.RowVersion != null)
        {
            punchItem.SetRowVersion(request.RowVersion);
        }
        else
        {
            unitOfWork.SetModified(punchItem);
        }
        
        punchItemRepository.Remove(punchItem);

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemDeletedIntegrationEventsAsync(punchItem, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
            
        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} deleted", punchItem.ItemNo, punchItem.Guid);

        try
        {
            await syncToPCS4Service.SyncPunchListItemDeleteAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Sync Delete on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
            return Unit.Value;
        }

        try
        {
            await checkListApiService.RecalculateCheckListStatusAsync(punchItem.Plant, punchItem.CheckListGuid, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Recalculate the CheckListStatus for CheckList with Guid {guid}", punchItem.CheckListGuid);
        }
            
        return Unit.Value;
    }

    private async Task<PunchItemDeletedIntegrationEvent> PublishPunchItemDeletedIntegrationEventsAsync(PunchItem punchItem, CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemDeletedIntegrationEvent(punchItem);

        await messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryDeletedIntegrationEvent(
            $"Punch item {punchItem.Category} {punchItem.ItemNo} deleted",
            punchItem.Guid,
            punchItem.CheckListGuid,
            new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
            punchItem.ModifiedAtUtc!.Value);

        await messageProducer.SendHistoryAsync(historyEvent, cancellationToken);

        return integrationEvent;
    }
}
