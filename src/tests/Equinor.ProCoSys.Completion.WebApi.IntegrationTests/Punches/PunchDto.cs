using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Punches;

public record PunchDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string RowVersion);
