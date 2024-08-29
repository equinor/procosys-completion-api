using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries.GetDuplicateInfo;

public record DuplicateInfoDto(
    CheckListDetailsDto CheckList, 
    IEnumerable<ResponsibleDto> Responsibles, 
    IEnumerable<TagFunctionDto> TagFunctions);
