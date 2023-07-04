using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public record PunchDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string RowVersion);
