using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;

public class ClearPunchItemCommandHandler(
    IPersonRepository personRepository,
    ISyncToPCS4Service syncToPCS4Service,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ICheckListApiService checkListApiService,
    ILogger<ClearPunchItemCommandHandler> logger)
    : PunchUpdateCommandBase, IRequestHandler<ClearPunchItemCommand, string>
{
    public async Task<string> Handle(ClearPunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;
        var currentPerson = await personRepository.GetCurrentPersonAsync(cancellationToken);
        punchItem.Clear(currentPerson);

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            "Punch item cleared",
            [],
            cancellationToken);

        punchItem.SetRowVersion(request.RowVersion);
        await unitOfWork.SaveChangesAsync(cancellationToken);
            
        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} cleared", punchItem.ItemNo, punchItem.Guid);

        try
        {
            await syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Sync Clear on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
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
}
