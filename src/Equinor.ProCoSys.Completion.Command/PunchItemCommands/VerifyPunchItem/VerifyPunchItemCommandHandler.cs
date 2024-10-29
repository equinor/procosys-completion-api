using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;

public class VerifyPunchItemCommandHandler(
    IPersonRepository personRepository,
    ISyncToPCS4Service syncToPCS4Service,
    IUnitOfWork unitOfWork,
    IMessageProducer messageProducer,
    ILogger<VerifyPunchItemCommandHandler> logger)
    : PunchUpdateCommandBase, IRequestHandler<VerifyPunchItemCommand, string>
{
    public async Task<string> Handle(VerifyPunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;

        var currentPerson = await personRepository.GetCurrentPersonAsync(cancellationToken);
        punchItem.Verify(currentPerson);

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            messageProducer,
            punchItem,
            "Punch item verified",
            [],
            false,
            cancellationToken);

        punchItem.SetRowVersion(request.RowVersion);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} verified", punchItem.ItemNo, punchItem.Guid);

        try
        {
            await syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while trying to Sync Verify on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
        }

        return punchItem.RowVersion.ConvertToString();
    }
}
