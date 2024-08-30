using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;

public record SearchResultDto(int MaxAvailable, IEnumerable<CheckListDto> Items);
