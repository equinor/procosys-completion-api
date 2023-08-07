using System;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

public record PunchItemDto(
    Guid Guid,
    string ProjectName,
    int ItemNo,
    string Description,
    string RowVersion);
