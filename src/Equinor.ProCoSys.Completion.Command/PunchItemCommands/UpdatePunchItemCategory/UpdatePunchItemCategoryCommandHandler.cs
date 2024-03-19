using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
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
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<UpdatePunchItemCategoryCommandHandler> _logger;

    public UpdatePunchItemCategoryCommandHandler(
        IPunchItemRepository punchItemRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<UpdatePunchItemCategoryCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UpdatePunchItemCategoryCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);

            var change = UpdateCategory(punchItem, request.Category);

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
                _integrationEventPublisher,
                punchItem,
                $"Punch item category changed to {request.Category}",
                [change],
                cancellationToken);

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncObjectUpdateAsync(SyncToPCS4Constants.PunchItem, integrationEvent, punchItem.Plant, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated as {PunchItemCategory}",
                punchItem.ItemNo,
                punchItem.Guid,
                punchItem.Category);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on update category of punch item with guid {PunchItemGuid}.", request.PunchItemGuid);
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
