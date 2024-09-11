using MediatR;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;

public class CreateLabelCommand : IRequest<string>
{
    public CreateLabelCommand(string text) => Text = text;

    public string Text { get; }
}
