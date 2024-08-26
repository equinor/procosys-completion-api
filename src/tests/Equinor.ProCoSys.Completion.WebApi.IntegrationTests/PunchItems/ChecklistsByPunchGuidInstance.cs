using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public record SourceCheckListDto(
    Guid ProCoSysGuid,
    string ProjectName,
    string FormularType,
    string FormularResp,
    string TagFunctionCode
);

public record CheckListDto(
    Guid ProCoSysGuid,
    short FormSheetNo,
    short FormSubSheetNo,
    string TagNo,
    string FormularStatus,
    string TagFunctionCode,
    string McPkgNo,
    string CommPkgNo
);

public record ChecklistsByPunchGuidInstance
(
    SourceCheckListDto SourceCheckList, 
    IList<CheckListDto> CheckLists
);
