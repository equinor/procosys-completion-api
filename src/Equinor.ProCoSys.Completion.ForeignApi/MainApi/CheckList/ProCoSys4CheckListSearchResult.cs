using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public record ProCoSys4CheckListSearchResult(IEnumerable<ProCoSys4CheckListSearchDto> Items, int MaxAvailable);
