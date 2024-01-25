using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.UpdateLabelAvailableFor;

public class UpdateLabelAvailableForCommand : IRequest<Result<Unit>>
{
    public UpdateLabelAvailableForCommand(string text, List<EntityTypeWithLabel> availableFor)
    {
        Text = text;
        AvailableFor = availableFor;
    }

    public string Text { get; }
    public List<EntityTypeWithLabel> AvailableFor { get; }
}
