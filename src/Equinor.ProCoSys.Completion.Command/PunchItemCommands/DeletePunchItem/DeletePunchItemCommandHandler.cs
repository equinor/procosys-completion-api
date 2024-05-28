using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;
using User = Equinor.ProCoSys.Completion.MessageContracts.User;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;

public class DeletePunchItemCommandHandler : IRequestHandler<DeletePunchItemCommand, Result<Unit>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageProducer _messageProducer;
    private readonly ILogger<DeletePunchItemCommandHandler> _logger;

    public DeletePunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IMessageProducer messageProducer,
        ILogger<DeletePunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeletePunchItemCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

            // Setting RowVersion before delete has 2 missions:
            // 1) Set correct Concurrency
            // 2) Ensure that _unitOfWork.SetAuditDataAsync can set ModifiedBy / ModifiedAt needed in published events
            punchItem.SetRowVersion(request.RowVersion);
            _punchItemRepository.Remove(punchItem);

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await PublishPunchItemDeletedIntegrationEventsAsync(punchItem, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //await _syncToPCS4Service.SyncObjectDeletionAsync(SyncToPCS4Constants.PunchItem, integrationEvent, punchItem.Plant, cancellationToken);
            await _syncToPCS4Service.SyncPunchListItemDeleteAsync(integrationEvent, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} deleted", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<Unit>(Unit.Value);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred on deletion of punch item with guid {PunchItemGuid}", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

    }

    private async Task<PunchItemDeletedIntegrationEvent> PublishPunchItemDeletedIntegrationEventsAsync(PunchItem punchItem, CancellationToken cancellationToken)
    {
        var integrationEvent = new PunchItemDeletedIntegrationEvent(punchItem);

        await _messageProducer.PublishAsync(integrationEvent, cancellationToken);

        var historyEvent = new HistoryDeletedIntegrationEvent(
            $"Punch item {punchItem.Category} {punchItem.ItemNo} deleted",
            punchItem.Guid,
            punchItem.CheckListGuid,
            new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
            punchItem.ModifiedAtUtc!.Value);

        await _messageProducer.SendHistoryAsync(historyEvent, cancellationToken);

        return integrationEvent;
    }
}
