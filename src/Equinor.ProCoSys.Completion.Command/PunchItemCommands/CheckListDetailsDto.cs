using System;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public record CheckListDetailsDto(
    Guid CheckListGuid,
    string ResponsibleCode,
    bool IsOwningTagVoided,
    Guid ProjectGuid)
{
    public CheckListDetailsDto(ProCoSys4CheckList proCoSys4CheckList)
        : this(
            proCoSys4CheckList.CheckListGuid, 
            proCoSys4CheckList.ResponsibleCode, 
            proCoSys4CheckList.IsVoided, 
            proCoSys4CheckList.ProjectGuid)
    {
    }
}
