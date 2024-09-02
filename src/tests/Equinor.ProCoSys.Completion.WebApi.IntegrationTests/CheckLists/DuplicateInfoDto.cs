using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.CheckLists;

public record DuplicateInfoDto(
    CheckListDetailsDto CheckList,
    IEnumerable<ResponsibleDto> Responsibles,
    IEnumerable<TagFunctionDto> TagFunctions);
