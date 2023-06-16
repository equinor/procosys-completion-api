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

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunch;

public class UpdatePunchCommandHandler : IRequestHandler<UpdatePunchCommand, Result<string>>
{
    private readonly IPunchRepository _punchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePunchCommandHandler> _logger;

    public UpdatePunchCommandHandler(
        IPunchRepository punchRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePunchCommandHandler> logger)
    {
        _punchRepository = punchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UpdatePunchCommand request, CancellationToken cancellationToken)
    {
        var punch = await _punchRepository.TryGetByGuidAsync(request.PunchGuid);
        if (punch == null)
        {
            throw new Exception($"Entity {nameof(Punch)} {request.PunchGuid} not found");
        }

        punch.Update(request.Title, request.Text);
        punch.SetRowVersion(request.RowVersion);
        punch.AddDomainEvent(new PunchUpdatedEvent(punch));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
            
        _logger.LogInformation($"Punch '{request.Title}' updated");
            
        return new SuccessResult<string>(punch.RowVersion.ConvertToString());
    }
}
