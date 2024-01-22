using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.EventPublishers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;

public class VerifyPunchItemCommandHandler : PunchUpdateCommandBase, IRequestHandler<VerifyPunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ISyncToPCS4Service _syncToPCS4Service;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ILogger<VerifyPunchItemCommandHandler> _logger;

    public VerifyPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        IPersonRepository personRepository,
        ISyncToPCS4Service syncToPCS4Service,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<VerifyPunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _personRepository = personRepository;
        _syncToPCS4Service = syncToPCS4Service;
        _unitOfWork = unitOfWork;
        _integrationEventPublisher = integrationEventPublisher;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(VerifyPunchItemCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var punchItem = await _punchItemRepository.GetAsync(request.PunchItemGuid, cancellationToken);
            if (punchItem is null)
            {
                throw new Exception($"Entity {nameof(PunchItem)} {request.PunchItemGuid} not found");
            }

            var currentPerson = await _personRepository.GetCurrentPersonAsync(cancellationToken);
            punchItem.Verify(currentPerson);

            // AuditData must be set before publishing events due to use of Created- and Modified-properties
            await _unitOfWork.SetAuditDataAsync();

            var integrationEvent = await PublishPunchItemUpdatedIntegrationEventsAsync(
                _integrationEventPublisher,
                punchItem,
                "Punch item verified",
                [],
                cancellationToken);

            punchItem.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _syncToPCS4Service.SyncObjectUpdateAsync(SyncToPCS4Service.PunchItem, integrationEvent, punchItem.Plant, cancellationToken);

            // todo 109356 add unit tests
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} verified", punchItem.ItemNo, punchItem.Guid);

            return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
        }
        catch (Exception)
        {
            _logger.LogError("Error occurred on verify of punch item with guid {PunchItemGuid}.", request.PunchItemGuid);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
