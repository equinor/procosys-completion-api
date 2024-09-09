using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.UpdateLabelAvailableFor;

public class UpdateLabelAvailableForCommandHandler : IRequestHandler<UpdateLabelAvailableForCommand, Unit>
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

    public async Task<Unit> Handle(UpdateLabelAvailableForCommand request, CancellationToken cancellationToken)
    {
        var existingLabel = await _labelRepository.GetByTextAsync(request.Text, cancellationToken);

        await MakeLabelAvailableFor(existingLabel, request.AvailableFor, cancellationToken);

        RemoveLabelAvailableFor(existingLabel, request.AvailableFor);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Label {Label} updated regarding entity types it is available for ", request.Text);

        return Unit.Value;
    }

    private static void RemoveLabelAvailableFor(Label label, List<EntityTypeWithLabel> availableFor)
    {
        for (var i = label.AvailableFor.Count - 1; i >= 0; i--)
        {
            var labelEntity = label.AvailableFor.ElementAt(i);
            if (!availableFor.Contains(labelEntity.EntityType))
            {
                label.RemoveLabelAvailableFor(labelEntity);
            }
        }
    }

    private async Task MakeLabelAvailableFor(
        Label label,
        List<EntityTypeWithLabel> availableFor,
        CancellationToken cancellationToken)
    {
        foreach (var entityType in availableFor)
        {
            if (EntityTypeNotAvailableFor(label, entityType))
            {
                var labelEntity = await _labelEntityRepository.GetByTypeAsync(entityType, cancellationToken);
                label.MakeLabelAvailableFor(labelEntity);
            }
        }
    }

    private static bool EntityTypeNotAvailableFor(Label label, EntityTypeWithLabel entityType)
        => label.AvailableFor.All(e => e.EntityType != entityType);
}
