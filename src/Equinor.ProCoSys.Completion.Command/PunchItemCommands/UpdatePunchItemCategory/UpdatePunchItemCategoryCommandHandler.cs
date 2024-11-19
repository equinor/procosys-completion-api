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

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;

public class UpdatePunchItemCategoryCommandHandler(
    ISyncToPCS4Service syncToPCS4Service,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ICheckListApiService checkListApiService,
    ILogger<UpdatePunchItemCategoryCommandHandler> logger)
    : PunchUpdateCommandBase, IRequestHandler<UpdatePunchItemCategoryCommand, string>
{
    public async Task<string> Handle(UpdatePunchItemCategoryCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;

        var change = UpdateCategory(punchItem, request.Category);

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            $"Punch item category changed to {request.Category}",
            [change],
            true,
            cancellationToken);

        punchItem.SetRowVersion(request.RowVersion);
        await unitOfWork.SaveChangesAsync(cancellationToken);


        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated as {PunchItemCategory}",
            punchItem.ItemNo,
            punchItem.Guid,
            punchItem.Category);

        try
        {
            await syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Sync Update Category on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
            return punchItem.RowVersion.ConvertToString();
        }

        try
        {
            await checkListApiService.RecalculateCheckListStatusAsync(punchItem.Plant, punchItem.CheckListGuid, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Recalculate the CheckListStatus for CheckList with Guid {guid}", punchItem.CheckListGuid);
        }

        return punchItem.RowVersion.ConvertToString();
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
