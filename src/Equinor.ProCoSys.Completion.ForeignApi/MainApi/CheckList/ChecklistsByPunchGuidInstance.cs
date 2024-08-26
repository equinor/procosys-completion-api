using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

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

public record CheckListsByPunchGuidInstance
(
    SourceCheckListDto SourceCheckList, 
    IList<CheckListDto> CheckLists
);
