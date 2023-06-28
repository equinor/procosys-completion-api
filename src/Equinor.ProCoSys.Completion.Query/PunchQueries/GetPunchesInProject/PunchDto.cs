using System;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;

public record PunchDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string RowVersion);
