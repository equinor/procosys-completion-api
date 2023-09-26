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

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommandHandler : IRequestHandler<RejectPunchItemCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectPunchItemCommandHandler> _logger;

    public RejectPunchItemCommandHandler(
        IPunchItemRepository punchItemRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        ILogger<RejectPunchItemCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(RejectPunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemRepository.GetByGuidAsync(request.PunchItemGuid);
        if (punchItem == null)
        {
            throw new Exception($"Entity {nameof(PunchItem)} {request.PunchItemGuid} not found");
        }

        var currentPerson = await _personRepository.GetCurrentPersonAsync();
        punchItem.Reject(currentPerson);
        punchItem.SetRowVersion(request.RowVersion);
        punchItem.AddDomainEvent(new PunchItemRejectedDomainEvent(punchItem));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} rejected", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }
}
