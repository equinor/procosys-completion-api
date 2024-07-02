using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;

public class UnverifyPunchItemCommandHandler : PunchUpdateCommandBase, IRequestHandler<UnverifyPunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageProducer _messageProducer;
    private readonly ILogger<UnverifyPunchItemCommandHandler> _logger;

    public UnverifyPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IMessageProducer messageProducer,
        ILogger<UnverifyPunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UnverifyPunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

        punchItem.Unverify();

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            _messageProducer,
            punchItem,
            "Punch item unverified",
            [],
            cancellationToken);

        punchItem.SetRowVersion(request.RowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} unverified", punchItem.ItemNo, punchItem.Guid);

        try
        {
            await _syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while trying to Sync Unverify on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
        }

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }
}
