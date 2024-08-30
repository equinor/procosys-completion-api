using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public class CheckListGuidsDto(List<Guid> proCoSysGuids)
{
    public List<Guid> ProCoSysGuids { get; } = proCoSysGuids;
}
