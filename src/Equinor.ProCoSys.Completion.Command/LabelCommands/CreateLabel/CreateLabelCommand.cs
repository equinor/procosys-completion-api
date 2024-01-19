using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;

public class CreateLabelCommand : IRequest<Result<string>>
{
    public CreateLabelCommand(string text) => Text = text;

    public string Text { get; }
}
