using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;
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
    IAttachmentRepository attachmentRepository,
    ILinkRepository linkRepository)
    : IRequestHandler<DeletePunchItemCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeletePunchItemCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
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

            var links = await linkRepository.GetAllByParentGuidAsync(request.PunchItemGuid, cancellationToken);
            foreach (var link in links)
            {
                linkRepository.Remove(link);
            }
            var punchItem = await punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);
            
            // Setting RowVersion before delete has 2 missions:
            // 1) Set correct Concurrency
            // 2) Ensure that _unitOfWork.SetAuditDataAsync can set ModifiedBy / ModifiedAt needed in published events
            punchItem.SetRowVersion(request.RowVersion);
            punchItemRepository.Remove(punchItem);

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await unitOfWork.SetAuditDataAsync();

            var integrationEvent = await PublishPunchItemDeletedIntegrationEventsAsync(punchItem, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await syncToPCS4Service.SyncPunchListItemDeleteAsync(integrationEvent, cancellationToken);
            
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            await syncToPCS4Service.SyncPunchListItemDeleteAsync(integrationEvent, cancellationToken);
            
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await checkListApiService.RecalculateCheckListStatus(punchItem.Plant, punchItem.CheckListGuid, cancellationToken);
            
            logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} deleted", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<Unit>(Unit.Value);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred on deletion of PunchListItem with guid {PunchItemGuid}", request.PunchItemGuid);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

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
