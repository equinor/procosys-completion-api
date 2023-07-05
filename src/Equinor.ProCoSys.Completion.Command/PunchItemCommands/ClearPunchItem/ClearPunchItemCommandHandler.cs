using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;

public class ClearPunchItemCommandHandler : IRequestHandler<ClearPunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearPunchItemCommandHandler> _logger;

    public ClearPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        ILogger<ClearPunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(ClearPunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemRepository.GetByGuidAsync(request.PunchItemGuid);
        if (punchItem == null)
        {
            throw new Exception($"Entity {nameof(PunchItem)} {request.PunchItemGuid} not found");
        }

        var currentPerson = await _personRepository.GetCurrentPersonAsync();
        punchItem.Clear(currentPerson);
        punchItem.SetRowVersion(request.RowVersion);
        punchItem.AddDomainEvent(new PunchItemClearedDomainEvent(punchItem, currentPerson.Guid));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} cleared", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }
}
