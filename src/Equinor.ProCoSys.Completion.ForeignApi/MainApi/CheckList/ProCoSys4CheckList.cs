using System;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public record ProCoSys4CheckList(
    Guid CheckListGuid,
    string FormularType,
    string FormularGroup,
    string ResponsibleCode,
    string TagRegisterCode,
    string TagRegisterDescription,
    string TagFunctionCode,
    string TagFunctionDescription,
    bool IsVoided,
    Guid ProjectGuid);
