using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public record PunchItemDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string RowVersion);
