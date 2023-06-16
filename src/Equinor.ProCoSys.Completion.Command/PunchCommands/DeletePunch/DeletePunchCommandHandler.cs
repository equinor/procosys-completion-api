using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunch;

public class DeletePunchCommandHandler : IRequestHandler<DeletePunchCommand, Result<Unit>>
{
    private readonly IPunchRepository _punchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePunchCommandHandler> _logger;

    public DeletePunchCommandHandler(
        IPunchRepository punchRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeletePunchCommandHandler> logger)
    {
        _punchRepository = punchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeletePunchCommand request, CancellationToken cancellationToken)
    {
        var punch = await _punchRepository.TryGetByGuidAsync(request.PunchGuid);
        if (punch == null)
        {
            throw new Exception($"Entity {nameof(Punch)} {request.PunchGuid} not found");
        }

        // Setting RowVersion before delete has 2 missions:
        // 1) Set correct Concurrency
        // 2) Trigger the update of modifiedBy / modifiedAt to be able to log who performed the deletion
        punch.SetRowVersion(request.RowVersion);
        _punchRepository.Remove(punch);
        punch.AddDomainEvent(new PunchDeletedEvent(punch));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"{nameof(Punch)} '{punch.Title}' deleted");

        return new SuccessResult<Unit>(Unit.Value);
    }
}
