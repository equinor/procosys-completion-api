using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;

/// <summary>
/// Represents a person performing an action (Clear, Verify, or Reject) on a punch item during import
/// </summary>
public record ActionByPerson(Person Person, DateTime ActionDate);
