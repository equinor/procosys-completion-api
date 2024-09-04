using System;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public record CheckListDetailsDto(
    Guid CheckListGuid,
    string ResponsibleCode,
    bool IsOwningTagVoided,
    Guid ProjectGuid)
{
    public CheckListDetailsDto(ProCoSys4CheckList pcs4CheckList)
        : this(
            pcs4CheckList.CheckListGuid, 
            pcs4CheckList.ResponsibleCode, 
            pcs4CheckList.IsVoided, 
            pcs4CheckList.ProjectGuid)
    {
    }
}
