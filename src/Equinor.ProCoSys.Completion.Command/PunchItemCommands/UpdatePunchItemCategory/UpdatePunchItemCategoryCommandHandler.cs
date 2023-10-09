using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;

public class UpdatePunchItemCategoryCommandHandler : IRequestHandler<UpdatePunchItemCategoryCommand, Result<string>>
{
    private readonly IPunchItemRepository _punchItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePunchItemCategoryCommandHandler> _logger;

    public UpdatePunchItemCategoryCommandHandler(
        IPunchItemRepository punchItemRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePunchItemCategoryCommandHandler> logger)
    {
        _punchItemRepository = punchItemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UpdatePunchItemCategoryCommand request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemRepository.GetByGuidAsync(request.PunchItemGuid);
        if (punchItem is null)
        {
            throw new Exception($"Entity {nameof(PunchItem)} {request.PunchItemGuid} not found");
        }

        punchItem.AddDomainEvent(new PunchItemCategoryUpdatedDomainEvent(
            punchItem,
            new Property<string>(
                nameof(PunchItem.Category),
                punchItem.Category.ToString(),
                request.Category.ToString())));

        punchItem.Category = request.Category;
        punchItem.SetRowVersion(request.RowVersion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Punch item '{PunchItemNo}' with guid {PunchItemGuid} updated", punchItem.ItemNo, punchItem.Guid);

        return new SuccessResult<string>(punchItem.RowVersion.ConvertToString());
    }
}
