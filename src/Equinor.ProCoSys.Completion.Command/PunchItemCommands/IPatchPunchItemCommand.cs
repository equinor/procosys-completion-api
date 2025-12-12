using System;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

/// <summary>
/// Interface for commands that patch a PunchItem using JsonPatchDocument.
/// Used to share validation logic between UpdatePunchItemCommand and ImportUpdatePunchItemCommand.
/// </summary>
public interface IPatchPunchItemCommand : IIsPunchItemCommand, ICanHaveRestrictionsViaCheckList
{
    JsonPatchDocument<PatchablePunchItem> PatchDocument { get; }
}
