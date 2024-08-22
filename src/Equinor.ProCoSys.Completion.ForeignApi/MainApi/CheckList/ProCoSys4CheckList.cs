using System;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public record ProCoSys4CheckList(
    Guid CheckListGuid,
    string ResponsibleCode,
    bool IsVoided,
    Guid ProjectGuid);
