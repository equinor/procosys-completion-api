using System;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;

/// <summary>
/// Represents a person performing an action (Clear, Verify, or Reject) on a punch item during import
/// </summary>
public record ActionByPerson(Guid PersonOid, DateTime ActionDate);
