using System;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchItemsInProject;

public record PunchDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string RowVersion);
