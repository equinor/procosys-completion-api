using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.VoidPunch;

public class VoidPunchCommandHandler : IRequestHandler<VoidPunchCommand, Result<string>>
{
    private readonly IPunchRepository _punchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VoidPunchCommandHandler> _logger;

    public VoidPunchCommandHandler(
        IPunchRepository punchRepository,
        IUnitOfWork unitOfWork,
        ILogger<VoidPunchCommandHandler> logger)
    {
        _punchRepository = punchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(VoidPunchCommand request, CancellationToken cancellationToken)
    {
        var punch = await _punchRepository.TryGetByGuidAsync(request.PunchGuid);
        if (punch == null)
        {
            throw new Exception($"Entity {nameof(Punch)} {request.PunchGuid} not found");
        }

        punch.IsVoided = true;
        punch.AddDomainEvent(new PunchVoidedEvent(punch));
        punch.SetRowVersion(request.RowVersion);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
            
        _logger.LogInformation($"Punch '{punch.Title}' voided");
            
        return new SuccessResult<string>(punch.RowVersion.ConvertToString());
    }
}
