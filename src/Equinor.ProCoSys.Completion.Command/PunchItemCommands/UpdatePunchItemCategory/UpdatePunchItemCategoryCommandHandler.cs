using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;

public class UpdatePunchItemCategoryCommandHandler : PunchUpdateCommandBase, IRequestHandler<UpdatePunchItemCategoryCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageProducer _messageProducer;
    private readonly ICheckListApiService _checkListApiService;
    private readonly ILogger<UpdatePunchItemCategoryCommandHandler> _logger;

    public UpdatePunchItemCategoryCommandHandler(
        IPunchItemRepository punchItemRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IMessageProducer messageProducer,
        ICheckListApiService checkListApiService,
        ILogger<UpdatePunchItemCategoryCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _checkListApiService = checkListApiService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UpdatePunchItemCategoryCommand request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

        var change = UpdateCategory(punchItem, request.Category);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
                _messageProducer,
                punchItem,
                $"Punch item category changed to {request.Category}",
                [change],
                cancellationToken);

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
            
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _checkListApiService.RecalculateCheckListStatus(punchItem.Plant, punchItem.CheckListGuid, cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated as {PunchItemCategory}",
                punchItem.ItemNo,
                punchItem.Guid,
                punchItem.Category);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred on update category of PunchListItem with guid {PunchItemGuid}", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private IChangedProperty UpdateCategory(PunchItem punchItem, Category category)
    {
        var change = new ChangedProperty<string>(
            nameof(PunchItem.Category),
            punchItem.Category.ToString(),
            category.ToString());

        punchItem.Category = category;

        return change;
    }
}
