using System;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public record ProCoSys4CheckListSearchDto(
    Guid CheckListGuid,
    string TagNo,
    string CommPkgNo,
    string McPkgNo,
    string FormularType,
    string FormularGroup,
    string Status,
    string ResponsibleCode,
    string TagRegisterCode,
    string TagRegisterDescription,
    string TagFunctionCode,
    string TagFunctionDescription,
    short? SheetNo,
    short? SubSheetNo,
    short? RevisionNo);
