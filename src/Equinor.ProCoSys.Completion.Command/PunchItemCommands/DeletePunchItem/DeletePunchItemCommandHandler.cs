using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;

public class DeletePunchItemCommandHandler : IRequestHandler<DeletePunchItemCommand, Result<Unit>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePunchItemCommandHandler> _logger;

    public DeletePunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        ILogger<DeletePunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
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
            // 2) Trigger the update of modifiedBy / modifiedAt to be able to log who performed the deletion
            punchItem.SetRowVersion(request.RowVersion);
            _punchItemRepository.Remove(punchItem);
            punchItem.AddDomainEvent(new PunchItemDeletedDomainEvent(punchItem));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // To be removed when sync to PCS 4 is no longer needed
            //---
            //TODO: Bør ikke opprette event på nytt her. 
            var integrationEvent = new PunchItemDeletedIntegrationEvent(new PunchItemDeletedDomainEvent(punchItem));
            await _syncToPCS4Service.SyncObjectDeletionAsync("PunchItem", integrationEvent, punchItem.Plant, cancellationToken);
            //---

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
