using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommandHandler : IRequestHandler<UpdatePunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePunchItemCommandHandler> _logger;

    public UpdatePunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UpdatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemRepository.GetByGuidAsync(request.PunchItemGuid);
        if (punchItem is null)
        {
            throw new Exception($"Entity {nameof(PunchItem)} {request.PunchItemGuid} not found");
        }

        punchItem.Update(request.Description);
        punchItem.SetRowVersion(request.RowVersion);
        punchItem.AddDomainEvent(new PunchItemUpdatedEvent(punchItem));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }
}
