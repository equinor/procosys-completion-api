using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.UpdateLabelAvailableFor;

public class UpdateLabelAvailableForCommandHandler : IRequestHandler<UpdateLabelAvailableForCommand, Result<Unit>>
{
    private readonly ILabelRepository _labelRepository;
    private readonly ILabelEntityRepository _labelEntityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateLabelAvailableForCommandHandler> _logger;

    public UpdateLabelAvailableForCommandHandler(
        ILabelRepository labelRepository,
        ILabelEntityRepository labelEntityRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateLabelAvailableForCommandHandler> logger)
    {
        _labelRepository = labelRepository;
        _labelEntityRepository = labelEntityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateLabelAvailableForCommand request, CancellationToken cancellationToken)
    {
        var label = await _labelRepository.GetByTextAsync(request.Text, cancellationToken);

        foreach (var entityType in request.AvailableFor)
        {
            if (label.AvailableFor.All(e => e.EntityType != entityType))
            {
                var labelEntity = await _labelEntityRepository.GetByTypeAsync(entityType, cancellationToken);
                label.MakeLabelAvailableFor(labelEntity);
            }
        }

        for (var i = label.AvailableFor.Count - 1; i >= 0; i--)
        {
            var labelEntity = label.AvailableFor.ElementAt(i);
            if (!request.AvailableFor.Contains(labelEntity.EntityType))
            {
                label.RemoveLabelAvailableFor(labelEntity);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usage for Label {Label} updated", request.Text);

        return new SuccessResult<Unit>(Unit.Value);
    }
}
