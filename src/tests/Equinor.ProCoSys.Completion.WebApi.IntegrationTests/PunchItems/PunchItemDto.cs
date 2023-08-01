using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public record PunchItemDto(
    Guid Guid,
    string ProjectName,
    int ItemNo,
    string Description,
    string RowVersion);
