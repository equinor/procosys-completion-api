using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;

public class CreateLabelCommand : IRequest<Result<string>>
{
    public CreateLabelCommand(string label) => Label = label;

    public string Label { get; }
}
