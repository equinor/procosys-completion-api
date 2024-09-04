using System;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;

public record CheckListDto(
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
    short? RevisionNo)
{
    public CheckListDto(ProCoSys4CheckListSearchDto pcs4Dto)
    : this(
        pcs4Dto.CheckListGuid, 
        pcs4Dto.TagNo, 
        pcs4Dto.CommPkgNo, 
        pcs4Dto.McPkgNo,
        pcs4Dto.FormularType,
        pcs4Dto.FormularGroup,
        pcs4Dto.Status,
        pcs4Dto.ResponsibleCode,
        pcs4Dto.TagRegisterCode,
        pcs4Dto.TagRegisterDescription,
        pcs4Dto.TagFunctionCode,
        pcs4Dto.TagFunctionDescription,
        pcs4Dto.SheetNo,
        pcs4Dto.SubSheetNo,
        pcs4Dto.RevisionNo)
    {
    }
}
