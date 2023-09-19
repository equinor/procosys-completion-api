using System;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public record ProCoSys4CheckList(
    string ResponsibleCode,
    bool IsVoided,
    Guid ProjectGuid);
