namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public record PunchItemPatchDto(
    string Description,
    string RowVersion);
