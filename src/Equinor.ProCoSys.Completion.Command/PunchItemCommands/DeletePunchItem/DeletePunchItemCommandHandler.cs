using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventPublishers.HistoryEvents;
using Equinor.ProCoSys.Completion.Command.EventPublishers.PunchItemEvents;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;

public class DeletePunchItemCommandHandler : IRequestHandler<DeletePunchItemCommand, Result<Unit>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPunchEventPublisher _punchEventPublisher;
    private readonly IHistoryEventPublisher _historyEventPublisher;
    private readonly ILogger<DeletePunchItemCommandHandler> _logger;

    public DeletePunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IPunchEventPublisher punchEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
        ILogger<DeletePunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _punchEventPublisher = punchEventPublisher;
        _historyEventPublisher = historyEventPublisher;
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

            var integrationEvent = await _punchEventPublisher.PublishDeletedEventAsync(punchItem, cancellationToken);
            await _historyEventPublisher.PublishDeletedEventAsync(
                punchItem.Plant,
                "Punch item deleted",
                punchItem.Guid,
                punchItem.CheckListGuid,
                new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
                punchItem.ModifiedAtUtc!.Value,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncObjectDeletionAsync("PunchItem", integrationEvent, punchItem.Plant, cancellationToken);

            // todo 109356 add unit tests
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} deleted", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<Unit>(Unit.Value);
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on deletion of punch item with guid {PunchItemGuid}.", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

    }
}
