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
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;

public class VerifyPunchItemCommandHandler : PunchUpdateCommandBase, IRequestHandler<VerifyPunchItemCommand, Result<string>>
{
    private readonly IPersonRepository _personRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageProducer _messageProducer;
    private readonly ILogger<VerifyPunchItemCommandHandler> _logger;

    public VerifyPunchItemCommandHandler(
        IPersonRepository personRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IMessageProducer messageProducer,
        ILogger<VerifyPunchItemCommandHandler> logger)
    {
        _personRepository = personRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(VerifyPunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;

        var currentPerson = await _personRepository.GetCurrentPersonAsync(cancellationToken);
        punchItem.Verify(currentPerson);

        // AuditData must be set before publishing events due to use of Created- and Modified-properties
        await _unitOfWork.SetAuditDataAsync();

        var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
            _messageProducer,
            punchItem,
            "Punch item verified",
            [],
            cancellationToken);

        punchItem.SetRowVersion(request.RowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} verified", punchItem.ItemNo, punchItem.Guid);

        try
        {
            await _syncToPCS4Service.SyncPunchListItemUpdateAsync(integrationEvent, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while trying to Sync Verify on PunchItemList with guid {PunchItemGuid}", request.PunchItemGuid);
        }

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }
}
