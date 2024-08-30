using System;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

namespace Equinor.ProCoSys.Completion.Query;

public record CheckListDetailsDto(
    Guid CheckListGuid,
    string FormularType,
    string ResponsibleCode,
    string TagRegisterCode,
    string TagRegisterDescription,
    string TagFunctionCode,
    string TagFunctionDescription,
    Guid ProjectGuid)
{
    public CheckListDetailsDto(ProCoSys4CheckList pcs4CheckList)
        : this(
            pcs4CheckList.CheckListGuid,
            pcs4CheckList.FormularType,
            pcs4CheckList.ResponsibleCode,
            pcs4CheckList.TagRegisterCode,
            pcs4CheckList.TagRegisterDescription,
            pcs4CheckList.TagFunctionCode,
            pcs4CheckList.TagFunctionDescription,
            pcs4CheckList.ProjectGuid)
    {
    }
}
