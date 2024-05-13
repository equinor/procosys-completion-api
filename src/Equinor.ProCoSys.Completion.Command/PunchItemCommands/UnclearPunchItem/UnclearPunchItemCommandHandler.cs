using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;

public class UnclearPunchItemCommandHandler : PunchUpdateCommandBase, IRequestHandler<UnclearPunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<UnclearPunchItemCommandHandler> _logger;

    public UnclearPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<UnclearPunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UnclearPunchItemCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

            punchItem.Unclear();

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
                _integrationEventPublisher,
                punchItem,
                "Punch item uncleared",
                [],
                cancellationToken);

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //await _syncToPCS4Service.SyncObjectUpdateAsync(SyncToPCS4Constants.PunchItem, integrationEvent, punchItem.Plant, cancellationToken);
            await _syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} uncleared", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on unclear of punch item with guid {PunchItemGuid}.", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
