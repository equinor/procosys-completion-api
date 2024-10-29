using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;

public class UnverifyPunchItemCommandHandler(
    ISyncToPCS4Service syncToPCS4Service,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ILogger<UnverifyPunchItemCommandHandler> logger)
    : PunchUpdateCommandBase, IRequestHandler<UnverifyPunchItemCommand, string>
{
    public async Task<string> Handle(UnverifyPunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;

        punchItem.Unverify();

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            "Punch item unverified",
            [],
            false,
            cancellationToken);

        punchItem.SetRowVersion(request.RowVersion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} unverified", punchItem.ItemNo, punchItem.Guid);

        try
        {
            await syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Sync Unverify on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
        }

        return punchItem.RowVersion.ConvertToString();
    }
}
