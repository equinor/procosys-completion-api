using System;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public record CheckListDetailsDto(
    Guid CheckListGuid,
    string ResponsibleCode,
    bool IsOwningTagVoided,
    Guid ProjectGuid);
