using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;

public class UnclearPunchItemCommandHandler : PunchUpdateCommandBase, IRequestHandler<UnclearPunchItemCommand, Result<string>>
{
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageProducer _messageProducer;
    private readonly ICheckListApiService _checkListApiService;
    private readonly ILogger<UnclearPunchItemCommandHandler> _logger;

    public UnclearPunchItemCommandHandler(
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IMessageProducer messageProducer,
        ICheckListApiService checkListApiService,
        ILogger<UnclearPunchItemCommandHandler> logger)
    {
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _checkListApiService = checkListApiService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UnclearPunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;

        punchItem.Unclear();

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            _messageProducer,
            punchItem,
            "Punch item uncleared",
            [],
            cancellationToken);

        punchItem.SetRowVersion(request.RowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} uncleared", punchItem.ItemNo, punchItem.Guid);

        try
        {
            await _syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while trying to Sync Unclear on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }

        try
        {
            await _checkListApiService.RecalculateCheckListStatusAsync(punchItem.Plant, punchItem.CheckListGuid, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while trying to Recalculate the CheckListStatus for CheckList with Guid {guid}", punchItem.CheckListGuid);
        }

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }
}
