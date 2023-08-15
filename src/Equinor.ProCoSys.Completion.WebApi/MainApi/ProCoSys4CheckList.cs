using System;

namespace Equinor.ProCoSys.Completion.WebApi.MainApi;

public record ProCoSys4CheckList(
    string ResponsibleCode,
    bool IsVoided,
    Guid ProjectGuid);
