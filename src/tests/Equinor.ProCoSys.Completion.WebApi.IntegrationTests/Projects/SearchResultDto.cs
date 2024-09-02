using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Projects;

public record SearchResultDto(int MaxAvailable, IEnumerable<CheckListDto> Items);
