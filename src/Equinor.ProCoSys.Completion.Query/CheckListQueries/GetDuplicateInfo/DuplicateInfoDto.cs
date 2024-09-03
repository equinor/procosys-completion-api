using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries.GetDuplicateInfo;

public record DuplicateInfoDto(
    CheckListDetailsDto CheckList, 
    IEnumerable<FormularTypeDto> FormularTypes, 
    IEnumerable<ResponsibleDto> Responsibles,
    IEnumerable<TagFunctionDto> TagFunctions);
