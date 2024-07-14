using System;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Tags;

public sealed record ProCoSys4Tag(int Id, Guid ProCoSysGuid, string TagNo);
