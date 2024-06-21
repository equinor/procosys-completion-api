using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public record PICheckListDto(
    Guid ProCoSysGuid,
    string ProjectName,
    string FormularType,
    string FormularResp
);

public record CheckListDto(
    Guid ProCoSysGuid,
    short FormSheetNo,
    short FormSubSheetNo,
    string TagNo,
    string FormularStatus
);

public record ChecklistsByPunchGuidInstance
(
    PICheckListDto PunchItemCheckList, IList<CheckListDto> CheckLists
);
