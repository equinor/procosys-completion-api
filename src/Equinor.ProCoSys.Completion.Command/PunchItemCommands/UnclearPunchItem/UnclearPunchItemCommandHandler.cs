using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventPublishers.HistoryEvents;
using Equinor.ProCoSys.Completion.Command.EventPublishers.PunchItemEvents;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;

public class UnclearPunchItemCommandHandler : IRequestHandler<UnclearPunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPunchEventPublisher _punchEventPublisher;
    private readonly IHistoryEventPublisher _historyEventPublisher;
    private readonly ILogger<UnclearPunchItemCommandHandler> _logger;

    public UnclearPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IPunchEventPublisher punchEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
        ILogger<UnclearPunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _punchEventPublisher = punchEventPublisher;
        _historyEventPublisher = historyEventPublisher;
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

            var integrationEvent = await _punchEventPublisher.PublishUpdatedEventAsync(punchItem, cancellationToken);
            await _historyEventPublisher.PublishUpdatedEventAsync(
                punchItem.Plant,
                "Punch item uncleared",
                punchItem.Guid,
                new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
                punchItem.ModifiedAtUtc!.Value,
                [],
                cancellationToken);

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncObjectUpdateAsync("PunchItem", integrationEvent, punchItem.Plant, cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} uncleared", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on update of punch item with guid {PunchItemGuid}.", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
