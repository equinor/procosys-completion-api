using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Projects;


public record PunchItemDto(
    Guid Guid,
    string ProjectName,
    int ItemNo,
    string Description,
    string RowVersion);
